�*
 "serve*2.16.08� 
:
dense_1/bias:VarHandleOp*
shape:*
dtype0
O
!dense_1/bias:/Read/ReadVariableOpReadVariableOpdense_1/bias:*
dtype0
@
dense_1/kernel:VarHandleOp*
shape
:*
dtype0
S
#dense_1/kernel:/Read/ReadVariableOpReadVariableOpdense_1/kernel:*
dtype0
8
dense/bias:VarHandleOp*
shape:*
dtype0
K
dense/bias:/Read/ReadVariableOpReadVariableOpdense/bias:*
dtype0
>
dense/kernel:VarHandleOp*
dtype0*
shape
:
O
!dense/kernel:/Read/ReadVariableOpReadVariableOpdense/kernel:*
dtype0

NoOpNoOp
�
ConstConst*
dtype0*�
value�B��"�
�
layer-0
layer_with_weights-0
layer-1
layer_with_weights-1
layer-2
non_trainable_variables

layers
	variables
trainable_variables
	keras_api
 
v
	non_trainable_variables


layers
	variables
trainable_variables
	keras_api

kernel
bias
v
non_trainable_variables

layers
	variables
trainable_variables
	keras_api

kernel
bias
 
 
 
 
 
 
 
 
 
 
[Y
VARIABLE_VALUEdense/kernel::06layer_with_weights-0/kernel/.ATTRIBUTES/VARIABLE_VALUE
WU
VARIABLE_VALUEdense/bias::04layer_with_weights-0/bias/.ATTRIBUTES/VARIABLE_VALUE
 
 
 
 
 
][
VARIABLE_VALUEdense_1/kernel::06layer_with_weights-1/kernel/.ATTRIBUTES/VARIABLE_VALUE
YW
VARIABLE_VALUEdense_1/bias::04layer_with_weights-1/bias/.ATTRIBUTES/VARIABLE_VALUE
*
saver_filenamePlaceholder*
dtype0
8
Const_1Const*
dtype0*
valueB B^s3://.*
9
RegexFullMatchRegexFullMatchsaver_filenameConst_1
5
Const_2Const*
valueB B.part*
dtype0
:
Const_3Const*
valueB B
_temp/part*
dtype0
;
SelectSelectRegexFullMatchConst_2Const_3*
T0
9

StringJoin
StringJoinsaver_filenameSelect*
N
4

num_shardsConst*
value	B :*
dtype0
1
Const_4Const*
dtype0*
value	B : 
C
ShardedFilenameShardedFilename
StringJoinConst_4
num_shards
�
SaveV2/tensor_namesConst*
dtype0*�
value�B�B6layer_with_weights-0/kernel/.ATTRIBUTES/VARIABLE_VALUEB4layer_with_weights-0/bias/.ATTRIBUTES/VARIABLE_VALUEB6layer_with_weights-1/kernel/.ATTRIBUTES/VARIABLE_VALUEB4layer_with_weights-1/bias/.ATTRIBUTES/VARIABLE_VALUEB_CHECKPOINTABLE_OBJECT_GRAPH
L
SaveV2/shape_and_slicesConst*
valueBB B B B B *
dtype0
�
SaveV2SaveV2ShardedFilenameSaveV2/tensor_namesSaveV2/shape_and_slices!dense/kernel:/Read/ReadVariableOpdense/bias:/Read/ReadVariableOp#dense_1/kernel:/Read/ReadVariableOp!dense_1/bias:/Read/ReadVariableOpConst*
dtypes	
2
.
Const_5Const*
dtype0*
valueB 
Q
&MergeV2Checkpoints/checkpoint_prefixesPackShardedFilename*
T0*
N
Y
MergeV2CheckpointsMergeV2Checkpoints&MergeV2Checkpoints/checkpoint_prefixesConst_5
B
IdentityIdentitysaver_filename^MergeV2Checkpoints*
T0
�
RestoreV2/tensor_namesConst*�
value�B�B6layer_with_weights-0/kernel/.ATTRIBUTES/VARIABLE_VALUEB4layer_with_weights-0/bias/.ATTRIBUTES/VARIABLE_VALUEB6layer_with_weights-1/kernel/.ATTRIBUTES/VARIABLE_VALUEB4layer_with_weights-1/bias/.ATTRIBUTES/VARIABLE_VALUEB_CHECKPOINTABLE_OBJECT_GRAPH*
dtype0
O
RestoreV2/shape_and_slicesConst*
valueBB B B B B *
dtype0
o
	RestoreV2	RestoreV2saver_filenameRestoreV2/tensor_namesRestoreV2/shape_and_slices*
dtypes	
2
*

Identity_1Identity	RestoreV2*
T0
L
AssignVariableOpAssignVariableOpdense/kernel:
Identity_1*
dtype0
,

Identity_2IdentityRestoreV2:1*
T0
L
AssignVariableOp_1AssignVariableOpdense/bias:
Identity_2*
dtype0
,

Identity_3IdentityRestoreV2:2*
T0
P
AssignVariableOp_2AssignVariableOpdense_1/kernel:
Identity_3*
dtype0
,

Identity_4IdentityRestoreV2:3*
T0
N
AssignVariableOp_3AssignVariableOpdense_1/bias:
Identity_4*
dtype0

NoOp_1NoOp
�

Identity_5Identitysaver_filename^AssignVariableOp^AssignVariableOp_1^AssignVariableOp_2^AssignVariableOp_3^NoOp_1*
T0 "�,
saver_filename:0
Identity:0
Identity_58"
saved_model_main_op :�
�
layer-0
layer_with_weights-0
layer-1
layer_with_weights-1
layer-2
non_trainable_variables

layers
	variables
trainable_variables
	keras_api"
_tf_keras_layer
"
_tf_keras_input_layer
�
	non_trainable_variables


layers
	variables
trainable_variables
	keras_api

kernel
bias"
_tf_keras_layer
�
non_trainable_variables

layers
	variables
trainable_variables
	keras_api

kernel
bias"
_tf_keras_layer
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
:2dense/kernel
:2
dense/bias
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
"
_generic_user_object
 :2dense_1/kernel
:2dense_1/bias