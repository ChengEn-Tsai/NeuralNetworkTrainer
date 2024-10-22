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

public interface INeuralNetwork
{
    void LoadDataset();
    void BuildModel();
    void TrainModel();
    void TestModel(float[] args);
    void SaveModel(string name);
    void LoadModel(string path);
}

public abstract class BaseNeuralNetwork : INeuralNetwork
{
    protected readonly int BatchSize;
    protected readonly bool Classification;
    protected DataFrame Data;
    protected readonly int Epochs;
    protected readonly string FeaturesToKeep;
    protected readonly string PathToDataset;
    protected readonly string TargetClass;
    protected readonly List<ILayer> Layers;
    protected Tensors Input;
    protected Tensors Output;
    protected IModel Model;

    public BaseNeuralNetwork(List<ILayer> layers, int epochs, int batchSize, bool isClassification, string targetClass,
        string pathToDataset = "", string featuresToKeep = null)
    {
        Console.WriteLine("Initializing BaseNeuralNetwork");
        Layers = layers;
        Epochs = epochs;
        BatchSize = batchSize;
        Classification = isClassification;
        TargetClass = targetClass;
        PathToDataset = pathToDataset;
        FeaturesToKeep = featuresToKeep;
    }
    public BaseNeuralNetwork(string[] layers, int epochs, int batchSize, bool isClassification, string targetClass,
        string pathToDataset = "", string featuresToKeep = null)
    {
        Console.WriteLine("Initializing BaseNeuralNetwork");
        Layers = string2Layers(layers);
        Epochs = epochs;
        BatchSize = batchSize;
        Classification = isClassification;
        TargetClass = targetClass;
        PathToDataset = pathToDataset;
        FeaturesToKeep = featuresToKeep;
    }

    public abstract void LoadDataset();
    public abstract void BuildModel();
    public abstract void TrainModel();
    public abstract void TestModel(float[] args);
    public abstract void SaveModel(string name);
    public abstract void LoadModel(string path);

    protected void ApplyLayers(Func<ILayer, Tensors, Tensors> applyFunction)
    {
        Console.WriteLine("Applying layers to the model");
        if (Layers == null || Layers.Count == 0)
        {
            throw new InvalidOperationException("No layers available to apply.");
        }

        string name = Layers[0].GetType().Name;
        if (name == "Conv1D" || name == "LSTM")
        {
            Shape shape = (Input.shape[1], 1);
            print(shape.rank);
            Output = tf.keras.layers.Reshape(shape).Apply(Input);
            Output = applyFunction(Layers[0], Output);
            Output = applyFunction(tf.keras.layers.Flatten(), Output);
        }
        else
        {
            Output = applyFunction(Layers[0], Input);
        }

        Console.WriteLine($"Applied layer: {Layers[0].GetType().Name}");

        foreach (var layer in Layers.GetRange(1, Layers.Count - 1))
        {

            name = layer.GetType().Name;
            if (name == "Conv1D" || name == "LSTM")
            {
                Shape shape = (Output.shape[1], 1);
                Output = tf.keras.layers.Reshape(shape).Apply(Output);
                Output = applyFunction(layer, Output);
                Output = applyFunction(tf.keras.layers.Flatten(), Output);
            }
            else
            {
                Output = applyFunction(layer, Output);
            }
            Console.WriteLine($"Applied layer: {layer.GetType().Name}");
        }
    }

    public int unique(DataFrameColumn series)
    {
        Console.WriteLine("Calculating number of unique elements in series");
        var already_seen_element = new List<object>();
        foreach (var element in series)
            if (!already_seen_element.Contains(element))
                already_seen_element.Add(element);

        Console.WriteLine("Unique elements count: " + already_seen_element.Count);
        return already_seen_element.Count;
    }

    public List<ILayer> string2Layers(string[] elements)
    {
        Console.WriteLine("Converting string array to list of layers");
        var layers = new List<ILayer>();
        foreach (var element in elements)
        {
            var parsed = element.Split(',');

            switch (parsed[0])
            {
                case "Dense":
                    {
                        if (parsed[2] == "") parsed[2] = null;
                        layers.Add(tf.keras.layers.Dense(int.Parse(parsed[1]), parsed[2]?.ToLower()));
                        Console.WriteLine($"Added Dense layer with units: {parsed[1]} and activation: {parsed[2]}");
                        break;
                    }
                case "Dropout":
                    {
                        layers.Add(tf.keras.layers.Dropout(float.Parse(parsed[1])));
                        Console.WriteLine($"Added Dropout layer with rate: {parsed[1]}");
                        break;
                    }
                case "Conv1D":
                    {
                        layers.Add(tf.keras.layers.Conv1D(int.Parse(parsed[1]),
                            int.Parse(parsed[2]),
                            padding: "same",
                            strides: 1));
                        Console.WriteLine($"Added Conv1D layer with filters: {parsed[1]}, kernel size: {parsed[2]}");
                        break;
                    }
                case "LSTM":
                    {
                        layers.Add(tf.keras.layers.LSTM(
                            int.Parse(parsed[1])
                        ));
                        Console.WriteLine($"Added LSTM layer with units: {parsed[1]}");
                        break;
                    }
                case "BatchNormalisation":
                    {
                        layers.Add(tf.keras.layers.BatchNormalization());
                        Console.WriteLine("Added BatchNormalization layer");
                        break;
                    }
            }
        }

        return layers;
    }
}

public class CsvNeuralNetwork : BaseNeuralNetwork
{
    public CsvNeuralNetwork(string[] layers, int epochs, int batchSize, bool isClassification, string targetClass,
        string pathToDataset = "", string featuresToKeep = null)
        : base(layers, epochs, batchSize, isClassification, targetClass, pathToDataset, featuresToKeep)
    {
        Console.WriteLine("Initializing CsvNeuralNetwork");
    }
    public CsvNeuralNetwork(List<ILayer> layers, int epochs, int batchSize, bool isClassification, string targetClass,
        string pathToDataset = "", string featuresToKeep = null)
        : base(layers, epochs, batchSize, isClassification, targetClass, pathToDataset, featuresToKeep)
    {
        Console.WriteLine("Initializing CsvNeuralNetwork");
    }

    

    public override void LoadDataset()
    {
        Console.WriteLine("Loading dataset from path: " + PathToDataset);
        var dataPath = Path.GetFullPath(PathToDataset);
        Data = DataFrame.LoadCsv(dataPath);
        Console.WriteLine("Dataset loaded successfully");

        var toKeep = new List<DataFrameColumn>();
        if (FeaturesToKeep != null)
        {
            Console.WriteLine("Filtering features to keep");
            foreach (var element in FeaturesToKeep.Split(','))
            {
                toKeep.Add(Data[element]);
            }
            toKeep.Add(Data[TargetClass]);
            Data = new DataFrame(toKeep.ToArray());
            Console.WriteLine("Features filtered successfully");
        }
    }

    public override void BuildModel()
    {
        Console.WriteLine("Building model");
        var features = Data.Clone();
        var label = features[TargetClass];
        features.Columns.Remove(TargetClass);

        Input = keras.Input(features.Columns.Count, BatchSize);
        Console.WriteLine("Input layer created");

        ApplyLayers((layer, input) => layer.Apply(input));


        Output = Classification
            ? keras.layers.Dense(unique(label), "softmax").Apply(Output)
            : keras.layers.Dense(1, "linear").Apply(Output);
        Console.WriteLine("Output layer created");

        Model = keras.Model(Input, Output);
        Console.WriteLine("Model constructed");

        Console.WriteLine("Model layout : ");
        Model.summary();

        var loss = Classification ? keras.losses.CategoricalCrossentropy() : keras.losses.MeanSquaredError();
        var metrics = Classification ? new[] { "acc" } : new[] { "mae" };
        Model.compile(keras.optimizers.Adam(), loss, metrics);
        Console.WriteLine("Model compiled with loss function and metrics");
    }

    public override void TrainModel()
    {
        Console.WriteLine("Starting model training");
        (var featuresNp, var labelNp) = DivideDataset(Data);

        Console.WriteLine("features shape : " + featuresNp.shape);
        Console.WriteLine("label shape : " + labelNp.shape);

        var parameters = new CallbackParams { Model = Model };

        var callbacks = new List<ICallback>
        {
            new EarlyStopping(parameters, patience: 50, restore_best_weights: true)
        };
        bool useMultiprocessing = Environment.GetEnvironmentVariable("USE_MULTIPROCESSING") == "true";
        Console.WriteLine($"Using multiprocessing: {useMultiprocessing}");
        Model.fit(featuresNp, labelNp, BatchSize, Epochs, validation_split: 0.4f, callbacks: callbacks, use_multiprocessing: useMultiprocessing);
        Console.WriteLine("Model training completed");
    }

    public override void TestModel(float[] args)
    {
        Console.WriteLine("Testing model with input arguments");
        var array = np.array(new float[1, args.Length]);
        for (var i = 0; i < args.Length; i++) array[0, i] = args[i];
        var prediction = Model.predict(array);
        Console.WriteLine("Prediction: " + prediction.ToString());
    }

    public override void SaveModel(string name)
    {
        Console.WriteLine("Saving model to path: ./{name}");
        Model.save($"./{name}");
        Console.WriteLine("Model saved successfully");
    }

    public override void LoadModel(string path)
    {
        Console.WriteLine("Loading model from path: " + path);
        Model = tf.keras.models.load_model(path);
        Console.WriteLine("Model loaded successfully");
    }

    private (NDArray, NDArray) DivideDataset(DataFrame df)
    {
        Console.WriteLine("Dividing dataset into features and label");
        var features = df.Clone();
        var label = new DataFrame(df[TargetClass]);
        features.Columns.Remove(TargetClass);

        label = OneHotEncodeColumn(label, TargetClass);
        features = CatEncoding(features);

        var featuresNp = NDArrayBuilder(features);
        var labelNp = NDArrayBuilder(label);
        Console.WriteLine("Dataset division completed");

        return (featuresNp, labelNp);
    }

    private NDArray NDArrayBuilder(DataFrame data)
    {
        Console.WriteLine("Building NDArray from DataFrame");
        var temp = Dataframe2numpy(data);
        var dimX = temp.Count;
        var dimY = temp[0].Count();
        var res = np.array(new float[dimX, dimY]);
        for (var i = 0; i < dimX; i++)
            for (var j = 0; j < dimY; j++)
                res[i, j] = temp[i][j];
        Console.WriteLine("NDArray built successfully");
        return res;
    }

    private DataFrame OneHotEncodeColumn(DataFrame df, string columnName)
    {
        Console.WriteLine("One-hot encoding column: " + columnName);
        var column = df.Columns[columnName];
        if (column.DataType != typeof(string))
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

        encodedDf.Columns.Remove(columnName);
        Console.WriteLine("One-hot encoding completed for column: " + columnName);

        return encodedDf;
    }

    private List<NDArray> Dataframe2numpy(DataFrame data)
    {
        Console.WriteLine("Converting DataFrame to list of NDArrays");
        var res = new List<NDArray>();

        foreach (var row in data.Rows)
        {
            var temp = row.ToArray().Select(Convert.ToSingle).ToArray();
            res.Add(np.array(temp, np.float32));
        }

        Console.WriteLine("Conversion to list of NDArrays completed");
        return res;
    }

    private DataFrame CatEncoding(DataFrame df)
    {
        Console.WriteLine("Categorically encoding DataFrame");
        for (var i = 0; i < df.Columns.Count; i++)
        {
            if (df.Columns[i] is StringDataFrameColumn stringColumn)
            {
                var uniqueValues = new Dictionary<string, float>();
                var intValues = new List<float>();

                foreach (var value in stringColumn)
                {
                    if (!uniqueValues.ContainsKey(value))
                        uniqueValues[value] = uniqueValues.Count;
                    intValues.Add(uniqueValues[value]);
                }

                var intColumn = new SingleDataFrameColumn(stringColumn.Name, intValues);
                df.Columns[i] = intColumn;
            }
            else
            {
                df.Columns[i] = new SingleDataFrameColumn(df.Columns[i].Name, df.Columns[i].Cast<float>());
            }
        }

        Console.WriteLine("Categorical encoding completed");
        return df;
    }
}
