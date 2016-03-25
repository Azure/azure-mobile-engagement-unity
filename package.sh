 
PACKAGEPATH="$PWD/Engagement.unitypackage"
rm $PACKAGEPATH

SRCDIR="$PWD/src"

#-1 Create Empty App to receive the package

UNITYPROJECTNAME="engagement-project"
UNITYPROJECT="$PWD/$UNITYPROJECTNAME"
rm -rf $UNITYPROJECT

echo "Creating $UNITYPROJECT"
UNITYAPP=/Applications/Unity/Unity.app/Contents/MacOS/Unity
$UNITYAPP -batchmode -quit -createProject $UNITYPROJECTNAME

#-2 Copy the plugin files to the project

cp -r src/EngagementPlugin "$UNITYPROJECT/Assets"

TARGETAZME="$UNITYPROJECT/Assets/EngagementPlugin"

#-3 Build the iOS Library

TARGETIOS="$TARGETAZME/iOS"
SRCIOSDIR="$PWD/src/iOS"

PROJECTFILENAME="EngagementUnity.xcodeproj"
BUILD_PATH="$PWD/~build"
BUILD_DIR="$BUILD_PATH/build-$TARGETAPP"
TMP_DIR="$BUILD_PATH/tmp-$TARGETAPP"

rm -rf "$BUILD_PATH"
rm -rf "$PROJECTDIR/$PROJECTFILENAME/build"
mkdir "$BUILD_PATH"
TARGETAPP="EngagementUnity"
CONFIG="Release"
xcodebuild -project "$SRCIOSDIR/$PROJECTFILENAME" -target "$TARGETAPP" -configuration "$CONFIG" BUILD_DIR="$BUILD_DIR" TMP_DIR="$TMP_DIR" 

#-4 Copy iOS artefacts

IOSLIBRARYNAME="libEngagementUnity.a"
rm "$TARGETIOS/$IOSLIBRARYNAME"
cp "$BUILD_DIR/$CONFIG-iphoneos/$IOSLIBRARYNAME" "$TARGETIOS/$IOSLIBRARYNAME"

rm -rf "$TARGETIOS/res"
mkdir "$TARGETIOS/res"

cp "$SRCIOSDIR/EngagementReach/res/close.png" "$TARGETIOS/res" 
cp "$SRCIOSDIR/EngagementReach/res/AEDefaultAnnouncementView.xib" "$TARGETIOS/res" 
cp "$SRCIOSDIR/EngagementReach/res/AEDefaultPollView.xib" "$TARGETIOS/res" 
cp "$SRCIOSDIR/EngagementReach/res/AENotificationView.xib" "$TARGETIOS/res" 
cp "$SRCIOSDIR/EngagementSDK/Classes/AEIdfaProvider.h" "$TARGETIOS"
cp "$SRCIOSDIR/EngagementSDK/Classes/AEIdfaProvider.m" "$TARGETIOS"

#-5 Build Android library

TARGETANDROID="$TARGETAZME/Android"
ANDROIDLIBRARYNAME="libEngagementUnity.aar"

rm -rf "$TARGETANDROID"
mkdir "$TARGETANDROID"

SAVEPWD=$PWD
cd "$PWD/src/Android"
./gradlew clean
./gradlew build
cd "$SAVEPWD"

cp "$SAVEPWD/src/Android/libEngagementUnity/build/outputs/aar/libEngagementUnity-release.aar" "$TARGETANDROID/$ANDROIDLIBRARYNAME"

#-7 Build the package by launching Unity from the command line


$UNITYAPP  -batchmode -quit -projectPath "$UNITYPROJECT" -exportPackage "Assets/EngagementPlugin" "$PACKAGEPATH"

#-8 Done
