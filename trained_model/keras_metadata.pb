
òroot"_tf_keras_layer*Ò{
  "name": "model",
  "trainable": true,
  "expects_training_arg": false,
  "dtype": "float32",
  "batch_input_shape": null,
  "autocast": false,
  "class_name": "Functional",
  "config": {
    "name": "model",
    "layers": [
      {
        "name": "input_1",
        "class_name": "InputLayer",
        "config": {
          "sparse": false,
          "ragged": false,
          "name": "input_1",
          "dtype": "float32",
          "batch_input_shape": {
            "class_name": "TensorShape",
            "items": [
              23,
              3
            ]
          }
        },
        "inbound_nodes": []
      },
      {
        "name": "dense",
        "class_name": "Dense",
        "config": {
          "units": 22,
          "activation": "relu",
          "use_bias": true,
          "kernel_initializer": {
            "class_name": "GlorotUniform",
            "config": {
              "seed": null
            }
          },
          "bias_initializer": {
            "class_name": "Zeros",
            "config": {}
          },
          "kernel_regularizer": null,
          "bias_regularizer": null,
          "kernel_constraint": null,
          "bias_constraint": null,
          "name": null,
          "dtype": "float32",
          "trainable": true
        },
        "inbound_nodes": [
          [
            "input_1",
            0,
            0
          ]
        ]
      },
      {
        "name": "dense_1",
        "class_name": "Dense",
        "config": {
          "units": 3,
          "activation": "softmax",
          "use_bias": true,
          "kernel_initializer": {
            "class_name": "GlorotUniform",
            "config": {
              "seed": null
            }
          },
          "bias_initializer": {
            "class_name": "Zeros",
            "config": {}
          },
          "kernel_regularizer": null,
          "bias_regularizer": null,
          "kernel_constraint": null,
          "bias_constraint": null,
          "name": null,
          "dtype": "float32",
          "trainable": true
        },
        "inbound_nodes": [
          [
            "dense",
            0,
            0
          ]
        ]
      }
    ],
    "input_layers": [
      [
        "input_1",
        0,
        0
      ]
    ],
    "output_layers": [
      [
        "dense_1",
        0,
        0
      ]
    ]
  },
  "shared_object_id": 1,
  "build_input_shape": {
    "class_name": "TensorShape",
    "items": [
      23,
      3
    ]
  }
}2
Øroot.layer-0"_tf_keras_input_layer*¨{"class_name":"InputLayer","name":"input_1","dtype":"float32","sparse":false,"ragged":false,"batch_input_shape":{"class_name":"TensorShape","items":[23,3]},"config":{"sparse":false,"ragged":false,"name":"input_1","dtype":"float32","batch_input_shape":{"class_name":"TensorShape","items":[23,3]}}}2
äroot.layer_with_weights-0"_tf_keras_layer*­{
  "name": "dense",
  "trainable": true,
  "expects_training_arg": false,
  "dtype": "float32",
  "batch_input_shape": null,
  "autocast": false,
  "input_spec": {
    "class_name": "InputSpec",
    "config": {
      "DType": null,
      "Shape": null,
      "Ndim": null,
      "MinNdim": 2,
      "MaxNdim": null,
      "Axes": {
        "-1": 3
      }
    },
    "shared_object_id": 2
  },
  "class_name": "Dense",
  "config": {
    "units": 22,
    "activation": "relu",
    "use_bias": true,
    "kernel_initializer": {
      "class_name": "GlorotUniform",
      "config": {
        "seed": null
      }
    },
    "bias_initializer": {
      "class_name": "Zeros",
      "config": {}
    },
    "kernel_regularizer": null,
    "bias_regularizer": null,
    "kernel_constraint": null,
    "bias_constraint": null,
    "name": null,
    "dtype": "float32",
    "trainable": true
  },
  "shared_object_id": 3,
  "build_input_shape": {
    "class_name": "TensorShape",
    "items": [
      23,
      3
    ]
  }
}2
êroot.layer_with_weights-1"_tf_keras_layer*³{
  "name": "dense_1",
  "trainable": true,
  "expects_training_arg": false,
  "dtype": "float32",
  "batch_input_shape": null,
  "autocast": false,
  "input_spec": {
    "class_name": "InputSpec",
    "config": {
      "DType": null,
      "Shape": null,
      "Ndim": null,
      "MinNdim": 2,
      "MaxNdim": null,
      "Axes": {
        "-1": 22
      }
    },
    "shared_object_id": 4
  },
  "class_name": "Dense",
  "config": {
    "units": 3,
    "activation": "softmax",
    "use_bias": true,
    "kernel_initializer": {
      "class_name": "GlorotUniform",
      "config": {
        "seed": null
      }
    },
    "bias_initializer": {
      "class_name": "Zeros",
      "config": {}
    },
    "kernel_regularizer": null,
    "bias_regularizer": null,
    "kernel_constraint": null,
    "bias_constraint": null,
    "name": null,
    "dtype": "float32",
    "trainable": true
  },
  "shared_object_id": 5,
  "build_input_shape": {
    "class_name": "TensorShape",
    "items": [
      23,
      22
    ]
  }
}2