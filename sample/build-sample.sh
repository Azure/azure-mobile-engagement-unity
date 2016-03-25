PROJECTNAME=SampleProject
UNITY=/Applications/Unity/Unity.app/Contents/MacOS/Unity
PROJECTPATH=$PWD/$PROJECTNAME
AZMEPACKAGEPATH=$PWD/../Engagement.unitypackage

rm -rf $PROJECTNAME
$UNITY -batchmode -quit -createProject $PROJECTNAME
$UNITY -batchmode -quit -projectPath $PROJECTPATH -importPackage $AZMEPACKAGEPATH

ASSETSPATH="$PROJECTNAME/Assets"
EDITORPATH="$ASSETSPATH/Editor"
mkdir $EDITORPATH

cp Sample.cs $ASSETSPATH
cp SampleSetup.cs $EDITORPATH

$UNITY  -projectPath $PROJECTPATH  -executeMethod SampleSetup.CreateSampleScene &