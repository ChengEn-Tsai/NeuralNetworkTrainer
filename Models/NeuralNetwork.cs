using Microsoft.Data.Analysis;
using Microsoft.ML;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.Callbacks;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;
using Tensorflow.Util;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace DotNetAssignment2
{
    internal class NeuralNetwork
    {
        private Tensors input;
        private Tensors output;
        private int batchSize;
        private int epochs;
        private bool classification;
        private string targetClass;
        private List<ILayer> layers;
        private string pathToDataset;
        private DataFrame df;
        private IModel model;
        private string featuresToKeep;
        public NeuralNetwork(List<ILayer> layers, int epochs, int batchSize, bool isClassification, string targetClass, string pathToDataset = "", string featuresToKeep = null)
        {
            this.layers = layers;
            this.epochs = epochs;
            this.batchSize = batchSize;
            this.classification = isClassification;
            this.targetClass = targetClass;
            this.pathToDataset = pathToDataset;
            this.featuresToKeep = featuresToKeep;
        }

        public NeuralNetwork(string[] layers, int epochs, int batchSize, bool isClassification, string targetClass,
            string path = "", string featuresToKeep = null)
        {
            this.layers = string2Layers(layers);
            this.epochs = epochs;
            this.batchSize = batchSize;
            this.classification = isClassification;
            this.pathToDataset = path;
            this.targetClass = targetClass;
            this.featuresToKeep  = featuresToKeep;
        }

        public List<ILayer> string2Layers(string[] elements)
        { // from a string array, return a list of layer for the model
            List<ILayer> layers = new List<ILayer>();
            foreach (string element in elements)
            { // each string is parsed following this pattern : layer, param_1, param_2
                string[] parsed = element.Split(',');
                
                switch (parsed[0])
                {
                    case "Dense": // example : Dense, nbNeurons, activation_func // works
                        {
                            if (parsed[2] == "") parsed[2] = null;
                            layers.Add(tf.keras.layers.Dense(units: int.Parse(parsed[1]), activation: parsed[2].ToLower()));
                            break;
                        }
                    case "Dropout": // example : Dropout, rate // works
                        {
                            layers.Add(tf.keras.layers.Dropout(rate: float.Parse(parsed[1])));
                            break;
                        }
                    case "Conv1D": // example : Conv1D, filters, kernel_size // works
                        {
                            layers.Add(tf.keras.layers.Conv1D(filters: int.Parse(parsed[1]),
                                kernel_size: int.Parse(parsed[2]),
                                padding: "same",
                                strides: 1));
                            break;
                        }
                    case "LSTM": //example : LSTM,units // works
                        {
                            layers.Add(tf.keras.layers.LSTM(
                                units: int.Parse(parsed[1])
                                ));
                            break;
                        }
                    case "BatchNormalisation": // example : BatchNormalisation // works
                        {
                            layers.Add(tf.keras.layers.BatchNormalization());
                            break;
                        }

                }
            }

            return layers;
        }
        public void loadDataset()
        {
            // can only load from a csv with coma 
            var dataPath = Path.GetFullPath(pathToDataset);
            df = DataFrame.LoadCsv(dataPath);
            List<DataFrameColumn> toKeep = new List<DataFrameColumn>();
            if (featuresToKeep != null)
            {   
                // we keep the features listed in featuresToKeep
                foreach (string element in featuresToKeep.Split(','))
                {
                    toKeep.Add(df[element]);
                }
                toKeep.Add(df[targetClass]);
                df = new DataFrame(toKeep.ToArray());
            }

            Console.WriteLine(df);

        }

        public void loadDataset(string csv)
        {
            // can only load from a csv with coma 
            df = DataFrame.LoadCsvFromString(csv);
            List<DataFrameColumn> toKeep = new List<DataFrameColumn>();
            if (featuresToKeep != null)
            {
                // we keep the features listed in featuresToKeep
                foreach (string element in featuresToKeep.Split(","))
                {
                    toKeep.Add(df[element]);
                }
                toKeep.Add(df[targetClass]);
                df = new DataFrame(toKeep.ToArray());
            }

            df = catEncoding(df);

            Console.WriteLine(df);
        }
        public void buildModel()
        {
            // we load the features and label
            var features = df.Clone();
            var label = df[targetClass];
            features.Columns.Remove(targetClass);
            // we build the model

            input = keras.Input(features.Columns.Count, batchSize);
            if (layers[0] is Conv1D || layers[0] is LSTM)
            {
                input = tf.expand_dims(input);
                Shape shape = (input.shape[1], 1);
                print(shape.rank);
                output = tf.keras.layers.Reshape(shape).Apply(input);
                output = layers[0].Apply(output);
                output = tf.keras.layers.Flatten().Apply(output);
            }
            else
            {
                output = layers[0].Apply(input);
            }

            foreach (ILayer layer in layers.GetRange(1, layers.Count - 1))
            {
                if (layer is Conv1D || layer is LSTM)
                {
                    Shape shape = (output.shape[1], 1);
                    print(shape.rank);
                    output = tf.keras.layers.Reshape(shape).Apply(output);
                    output = layer.Apply(output);
                    output = tf.keras.layers.Flatten().Apply(output);
                }
                else
                {
                    output = layer.Apply(output);
                }

            }

            if (classification)
            {
                output = keras.layers.Dense(units: unique(label), activation: "softmax").Apply(output);
            }
            else
            {
                output = keras.layers.Dense(units: 1, activation: "linear").Apply(output);
            }

            model = keras.Model(input, output);

            // we compile the model
            if (classification)
            {
                model.compile(keras.optimizers.Adam(), keras.losses.CategoricalCrossentropy(), metrics: new[] { "acc" });
            }
            else
            {
                model.compile(keras.optimizers.Adam(), keras.losses.MeanSquaredError(), metrics: new[] { "mae" });
            }
            Console.WriteLine(model.Layers);
            model.summary();

        }

        public void trainModel()
        {
            // we load the features and label
            NDArray featuresNp;
            NDArray labelNp;
            if (classification) (featuresNp, labelNp) = divideDataset(df);
            else (featuresNp, labelNp) = divideDatasetReg(df);
            print(labelNp);
            print(featuresNp);
            CallbackParams parameters = new CallbackParams();
            parameters.Model = model;

            var callbacks = new List<ICallback>
            {
                new EarlyStopping(parameters, monitor: "val_loss", patience: 50, restore_best_weights:true),


            };
            model.fit(featuresNp, labelNp, batch_size: batchSize, epochs: epochs, verbose: 1, validation_split: 0.2f, callbacks: callbacks, use_multiprocessing: true);
        }

        public void testModel(float[] args)
        {
            // allows the user to test a model on a manually entered list of features.
            NDArray array = np.array(new float[1, args.Length]);
            for (int i = 0; i < args.Length; i++)
            {
                array[0, i] = args[i];
            }
            var prediction = model.predict(array);
            print(prediction);
        }

        public void saveModel(string name)
        {// save the model inside a file
            model.save($"./{name}");
        }

        public void loadModel(string path)
        {
            // load a model from a path
            model = tf.keras.models.load_model(path);
        }

        #region data management
        public int unique(DataFrameColumn series)
        {// return the number of unique element in a series
            List<object> already_seen_element = new List<object>();
            foreach (var element in series)
            {
                if (!already_seen_element.Contains(element))
                {
                    already_seen_element.Add(element);
                }
            }

            return already_seen_element.Count;
        }


        public List<NDArray> Dataframe2numpy(DataFrame data)
        {
            // Convert a dataframe to a list of NDarray (of floats)
            List<NDArray> res = new List<NDArray>();

            foreach (DataFrameRow row in data.Rows)
            {
                // Convert row elements to float
                var temp = row.ToArray().Select(Convert.ToSingle).ToArray();

                // Create an NDArray of floats
                res.Add(np.array(temp, dtype: np.float32));
            }

            return res;
        }

        public NDArray NDArrayBuilder(DataFrame data)
        {
            List<NDArray> temp = Dataframe2numpy(data);
            int dimX = temp.Count;
            int dimY = temp[0].Count();
            NDArray res = np.array(new float[dimX, dimY]);
            for (int i = 0; i < dimX; i++)
            {
                for (int j = 0; j < dimY; j++)
                {
                    res[i, j] = temp[i][j];
                }
            }
            return res;
        }

        public NDArray series2numpy(DataFrameColumn series)
        {
            List<string> temp = new List<string>();

            foreach (var elem in series)
            {
                temp.Add(elem.ToString());
            }

            return np.array(temp.ToArray());
        }

        public NDArray catEncoding(DataFrameColumn nd)
        {//encode a NDArray from string to integer
            long dim = nd.Length;
            NDArray res = np.array(new int[dim]);
            Dictionary<object, int> dict = new Dictionary<object, int>();
            int counter = 0;
            for (int i = 0; i < dim; i++)
            {
                if (dict.ContainsKey(nd[i]))
                {
                    res[i] = dict[nd[i]];
                }
                else
                {
                    counter++;
                    dict.Add(nd[i], counter);
                    res[i] = counter;
                }
            }
            return res;
        }

        public DataFrame catEncoding(DataFrame df)
        {// encode all the string column into integer from a dataframe
            for (int i = 0; i < df.Columns.Count; i++)
            {
                if (df.Columns[i] is StringDataFrameColumn stringColumn)
                {
                    // Create a mapping dictionary to assign unique integer values to strings
                    var uniqueValues = new Dictionary<string, float>();
                    var intValues = new List<float>();

                    foreach (var value in stringColumn)
                    {
                        if (!uniqueValues.ContainsKey(value))
                        {
                            uniqueValues[value] = uniqueValues.Count; // Assign a new unique integer
                        }
                        intValues.Add(uniqueValues[value]);
                    }

                    // Create a new Int32DataFrameColumn and replace the string column
                    SingleDataFrameColumn intColumn = new SingleDataFrameColumn(stringColumn.Name, intValues);
                    df.Columns[i] = intColumn;
                }
                else
                {
                    df.Columns[i] = new SingleDataFrameColumn(df.Columns[i].Name, df.Columns[i].Cast<float>());
                }
            }
            return df;
        }

        public DataFrame OneHotEncodeColumn(DataFrame df, string columnName)
        {
            DataFrameColumn column = df.Columns[columnName];
            if (column.DataType != typeof(string)) // we convert the column to string
            {
                List<string> elements = new List<string>();
                foreach (var element in column)
                {
                    elements.Add(element.ToString());
                }
                StringDataFrameColumn converted = new StringDataFrameColumn(columnName, elements);
                df.Columns.Remove(columnName);
                df.Columns.Add(converted);
                column = df.Columns[columnName];
            }

            DataFrame encodedDf = df.Clone();
            StringDataFrameColumn stringColumn = (StringDataFrameColumn)column;
            Dictionary<string, int> uniqueValues = new Dictionary<string, int>();

            // Create a mapping dictionary to assign unique integer values to strings
            int counter = 0;
            for (int i = 0; i < stringColumn.Length; i++)
            {
                string value = stringColumn[i];
                if (!uniqueValues.ContainsKey(value))
                {
                    uniqueValues[value] = counter;
                    counter++;
                }
            }

            // Create new columns for each unique value and assign binary values
            foreach (var kvp in uniqueValues)
            {
                string uniqueValue = kvp.Key;
                int uniqueValueIndex = kvp.Value;

                string newColumnName = $"{columnName}_{uniqueValue}";
                BooleanDataFrameColumn newColumn = new BooleanDataFrameColumn(newColumnName, df.Rows.Count);

                for (int i = 0; i < stringColumn.Length; i++)
                {
                    string value = stringColumn[i];
                    bool isUniqueValue = value == uniqueValue;
                    newColumn[i] = isUniqueValue;
                }

                encodedDf.Columns.Add(newColumn);
            }

            // Remove the original column
            encodedDf.Columns.Remove(columnName);

            return encodedDf;
        }

        public (NDArray, NDArray) divideDataset(DataFrame df)
        {
            // used for classification only. It divides the dataset into 2 Dataframes : the features and the label. It uses one hot encoding for the label
            //1. we divide the dataset into features and label
            var features = df.Clone();
            var label = new DataFrame(df[targetClass]);
            features.Columns.Remove(targetClass);

            //2. we encode the label and the features
            label = OneHotEncodeColumn(label, targetClass);
            features = catEncoding(features);

            //3. we convert the dataframes into numpy array
            var featuresNp = NDArrayBuilder(features);
            var labelNp = NDArrayBuilder(label);

            return (featuresNp, labelNp);
        }

        public (NDArray, NDArray) divideDatasetReg(DataFrame df)
        {
            // for regression only. Same thing as divideDataset, but no one hot encoding for the label
            //1. we divide the dataset into features and label
            var features = df.Clone();
            var label = new DataFrame(df[targetClass]);
            features.Columns.Remove(targetClass);

            //2. we encode the label and the features
            features = catEncoding(features);

            //3. we convert the dataframes into numpy array
            var featuresNp = NDArrayBuilder(features);
            var labelNp = NDArrayBuilder(label);

            return (featuresNp, labelNp);
        }
        #endregion
    }
}
