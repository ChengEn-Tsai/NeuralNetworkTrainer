using Microsoft.Data.Analysis;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.Callbacks;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.NumPy;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace DotNetAssignment2;

public class NeuralNetwork
{
    private readonly int batchSize;
    private readonly bool classification;
    private DataFrame df;
    private readonly int epochs;
    private readonly string featuresToKeep;
    private Tensors input;
    private readonly List<ILayer> layers;
    private IModel model;
    private Tensors output;
    private readonly string pathToDataset;
    private readonly string targetClass;

    public NeuralNetwork(List<ILayer> layers, int epochs, int batchSize, bool isClassification, string targetClass,
        string pathToDataset = "", string featuresToKeep = null)
    {
        this.layers = layers;
        this.epochs = epochs;
        this.batchSize = batchSize;
        classification = isClassification;
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
        classification = isClassification;
        pathToDataset = path;
        this.targetClass = targetClass;
        this.featuresToKeep = featuresToKeep;
    }

    public List<ILayer> string2Layers(string[] elements)
    {
        // from a string array, return a list of layer for the model
        var layers = new List<ILayer>();
        foreach (var element in elements)
        {
            // each string is parsed following this pattern : layer, param_1, param_2
            var parsed = element.Split(',');

            switch (parsed[0])
            {
                case "Dense": // example : Dense, nbNeurons, activation_func // works
                {
                    if (parsed[2] == "") parsed[2] = null;
                    layers.Add(tf.keras.layers.Dense(int.Parse(parsed[1]), parsed[2].ToLower()));
                    break;
                }
                case "Dropout": // example : Dropout, rate // works
                {
                    layers.Add(tf.keras.layers.Dropout(float.Parse(parsed[1])));
                    break;
                }
                case "Conv1D": // example : Conv1D, filters, kernel_size // works
                {
                    layers.Add(tf.keras.layers.Conv1D(int.Parse(parsed[1]),
                        int.Parse(parsed[2]),
                        padding: "same",
                        strides: 1));
                    break;
                }
                case "LSTM": //example : LSTM,units // works
                {
                    layers.Add(tf.keras.layers.LSTM(
                        int.Parse(parsed[1])
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
        var toKeep = new List<DataFrameColumn>();
        if (featuresToKeep != null)
        {
            // we keep the features listed in featuresToKeep
            foreach (var element in featuresToKeep.Split(',')) toKeep.Add(df[element]);
            toKeep.Add(df[targetClass]);
            df = new DataFrame(toKeep.ToArray());
        }

        Console.WriteLine(df);
    }

    public void loadDataset(string csv)
    {
        // can only load from a csv with coma 
        df = DataFrame.LoadCsvFromString(csv);
        var toKeep = new List<DataFrameColumn>();
        if (featuresToKeep != null)
        {
            // we keep the features listed in featuresToKeep
            foreach (var element in featuresToKeep.Split(",")) toKeep.Add(df[element]);
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

        foreach (var layer in layers.GetRange(1, layers.Count - 1))
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

        if (classification)
            output = keras.layers.Dense(unique(label), "softmax").Apply(output);
        else
            output = keras.layers.Dense(1, "linear").Apply(output);

        model = keras.Model(input, output);

        // we compile the model
        if (classification)
            model.compile(keras.optimizers.Adam(), keras.losses.CategoricalCrossentropy(), new[] { "acc" });
        else
            model.compile(keras.optimizers.Adam(), keras.losses.MeanSquaredError(), new[] { "mae" });
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
        var parameters = new CallbackParams();
        parameters.Model = model;

        var callbacks = new List<ICallback>
        {
            new EarlyStopping(parameters, patience: 50, restore_best_weights: true)
        };
        model.fit(featuresNp, labelNp, batchSize, epochs, validation_split: 0.2f, callbacks: callbacks,
            use_multiprocessing: true);
    }

    public void testModel(float[] args)
    {
        // allows the user to test a model on a manually entered list of features.
        var array = np.array(new float[1, args.Length]);
        for (var i = 0; i < args.Length; i++) array[0, i] = args[i];
        var prediction = model.predict(array);
        print(prediction);
    }

    public void saveModel(string name)
    {
        // save the model inside a file
        model.save($"./{name}");
    }

    public void loadModel(string path)
    {
        // load a model from a path
        model = tf.keras.models.load_model(path);
    }

    #region data management

    public int unique(DataFrameColumn series)
    {
        // return the number of unique element in a series
        var already_seen_element = new List<object>();
        foreach (var element in series)
            if (!already_seen_element.Contains(element))
                already_seen_element.Add(element);

        return already_seen_element.Count;
    }


    public List<NDArray> Dataframe2numpy(DataFrame data)
    {
        // Convert a dataframe to a list of NDarray (of floats)
        var res = new List<NDArray>();

        foreach (var row in data.Rows)
        {
            // Convert row elements to float
            var temp = row.ToArray().Select(Convert.ToSingle).ToArray();

            // Create an NDArray of floats
            res.Add(np.array(temp, np.float32));
        }

        return res;
    }

    public NDArray NDArrayBuilder(DataFrame data)
    {
        var temp = Dataframe2numpy(data);
        var dimX = temp.Count;
        var dimY = temp[0].Count();
        var res = np.array(new float[dimX, dimY]);
        for (var i = 0; i < dimX; i++)
        for (var j = 0; j < dimY; j++)
            res[i, j] = temp[i][j];
        return res;
    }

    public NDArray series2numpy(DataFrameColumn series)
    {
        var temp = new List<string>();

        foreach (var elem in series) temp.Add(elem.ToString());

        return np.array(temp.ToArray());
    }

    public NDArray catEncoding(DataFrameColumn nd)
    {
        //encode a NDArray from string to integer
        var dim = nd.Length;
        var res = np.array(new int[dim]);
        var dict = new Dictionary<object, int>();
        var counter = 0;
        for (var i = 0; i < dim; i++)
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

        return res;
    }

    public DataFrame catEncoding(DataFrame df)
    {
        // encode all the string column into integer from a dataframe
        for (var i = 0; i < df.Columns.Count; i++)
            if (df.Columns[i] is StringDataFrameColumn stringColumn)
            {
                // Create a mapping dictionary to assign unique integer values to strings
                var uniqueValues = new Dictionary<string, float>();
                var intValues = new List<float>();

                foreach (var value in stringColumn)
                {
                    if (!uniqueValues.ContainsKey(value))
                        uniqueValues[value] = uniqueValues.Count; // Assign a new unique integer
                    intValues.Add(uniqueValues[value]);
                }

                // Create a new Int32DataFrameColumn and replace the string column
                var intColumn = new SingleDataFrameColumn(stringColumn.Name, intValues);
                df.Columns[i] = intColumn;
            }
            else
            {
                df.Columns[i] = new SingleDataFrameColumn(df.Columns[i].Name, df.Columns[i].Cast<float>());
            }

        return df;
    }

    public DataFrame OneHotEncodeColumn(DataFrame df, string columnName)
    {
        var column = df.Columns[columnName];
        if (column.DataType != typeof(string)) // we convert the column to string
        {
            var elements = new List<string>();
            foreach (var element in column) elements.Add(element.ToString());
            var converted = new StringDataFrameColumn(columnName, elements);
            df.Columns.Remove(columnName);
            df.Columns.Add(converted);
            column = df.Columns[columnName];
        }

        var encodedDf = df.Clone();
        var stringColumn = (StringDataFrameColumn)column;
        var uniqueValues = new Dictionary<string, int>();

        // Create a mapping dictionary to assign unique integer values to strings
        var counter = 0;
        for (var i = 0; i < stringColumn.Length; i++)
        {
            var value = stringColumn[i];
            if (!uniqueValues.ContainsKey(value))
            {
                uniqueValues[value] = counter;
                counter++;
            }
        }

        // Create new columns for each unique value and assign binary values
        foreach (var kvp in uniqueValues)
        {
            var uniqueValue = kvp.Key;
            var uniqueValueIndex = kvp.Value;

            var newColumnName = $"{columnName}_{uniqueValue}";
            var newColumn = new BooleanDataFrameColumn(newColumnName, df.Rows.Count);

            for (var i = 0; i < stringColumn.Length; i++)
            {
                var value = stringColumn[i];
                var isUniqueValue = value == uniqueValue;
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