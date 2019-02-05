# ACTION KEYFRAMES GUIDE
A guide to scripting action keyframes in VizRT

## General:
* `;` Are used to seperate multiple commands in a single keyframe.

## Stage:
* `MAIN_SCENE*STAGE*DIRECTOR*directorname` Reference the first director named *directorname* in the main scene's STAGE.
* `RENDERER*STAGE*DIRECTOR*directorname` Reference the first director named *directorname* in the loaded scene's STAGE.
* `THIS_SCENE*STAGE*DIRECTOR*directorname` Reference the first director named *directorname* in the current scene' STAGE.
  * *First meaning top most in order and top most in parentage.*
  * *Since `THIS_DIRECTOR` is shorter than `THIS_SCENE...` I will use `THIS_DIRECTOR` in the examples bellow. But note that either can be used, and `THIS_SCENE..` is sometimes better suited.*

## Director:
* `THIS_DIRECTOR` Reference the director the keyframe is currently in.
* `THIS_DIRECTOR*ACTION*actionname*KEY*$keyname*TIME SET F20;` Moves the first action keyframe named *keyname* in the action named *actionname* in the current director to frame 20 on the timeline.
* `THIS_DIRECTOR CONTINUE;` Runs a continue command on the currect director.
* `THIS_DIRECTOR CONTINUE REVERSE;` Runs a continue command in reverse on the currect director.
* `THIS_DIRECTOR*KEY*$keyname*TIME SET F20;` Moves the keyframe named *keyname* in the current director to frame 20 on the timeline. 
* `THIS_DIRECTOR SHOW F20;` Places the timeline of the current director at frame 20. 
* `THIS_DIRECTOR SHOW $tagname;` Places the timeline of the current director at the same position as the first Stop/Tag/Pause point named *tagname*.
* `THIS_DIRECTOR START;` Runs a starts command on the currect director.
* ~~`THIS_SCENE*GFX*1*STAGE START` Starts graphic channle 1~~ Has been removed since version 3.5 I believe.

## Tree:
* `THIS_SCENE*TREE*$containername` Reference the first container named *containername* in the current scene TREE.
  * *First meaning top most in order and top most in parentage.*
* `THIS_SCENE*TREE*$containername*TRANSFORMATION*POSITION*Y SET 45.0;` Sets container named *containername* y-position to 45.0.
* `THIS_SCENE*TREE*$containername*ACTIVE SET 0;` Sets ... visibility to off.
* `THIS_SCENE*TREE*$containername*FUNCTION*Omo*vis_con SET 0;` Sets ... omo selector to 0.
* `THIS_SCENE*TREE*$containername*FUNCTION*Grid*num_row SET 5;` Changes the ... row set in a grid arrange to 5.
* `THIS_SCENE*TREE*$containername*FUNCTION*Maxsize*WIDTH_X SET 800.0;` Changes the ... maxsize width to 800.0.
* `THIS_SCENE*TREE*$containername*FUNCTION*Maxsize*DEFAULT_SCALE_Y SET .4;` Changes the ... maxsize Y scale to .4.
* `THIS_SCENE*TREE*$containername*GEOM*height SET 200.0;` Changes the ... geometry height to 200.0.
  * This can also be written using the vizid(*#123456*), but since that always changes I would advise against it. `THIS_SCENE*TREE*$containername*GEOM*#123456*height SET 200.0`
* `THIS_SCENE*TREE*$containername*SCRIPT INVOKE scriptsub;` Invokes the subroutine named *scriptsub* in the script plugin in the first container named *containername.*




ControlAction Table:
-----------------------------------------

THIS_SCENE*STAGE*DIRECTOR*directorname*ACTION*A*KEY*$keyname*TIME SET F3000; 
THIS_SCENE*STAGE*DIRECTOR*directorname*KEY*$keyname*TIME SET F3000;

THIS_SCENE*STAGE*DIRECTOR*directorname*ACTION*A*KEY*$keyname*TIME SET F53;  
THIS_SCENE*STAGE*DIRECTOR*directorname*KEY*$keyname*TIME SET F59;


-----------------------------------------------------------------------
THIS_SCENE*STAGE*DIRECTOR*directorName SHOW $tagName;						= jump to this tags point "tagName"
THIS_SCENE*STAGE*DIRECTOR*directorName CONTINUE




-----------------------------------------------------------------------
MAIN_SCENE*STAGE*DIRECTOR*nameOfToggleDir,AO$current$nameOfCallupPgDir GOTO_TRIO $tagName1 $tagName2;		= in Main TransLogic Scene finds
MAIN_SCENE*STAGE*DIRECTOR*nameOfToggleDir,BO$current$nameOfCallupPgDir GOTO_TRIO $tagName1 $tagName2;		  Callup Scene and goes to
												                    tagName1 and plays to tageName2


-----------------------------------------------------------------------
RENDERER*TREE*$groupname*FUNCTION*Toggle*switch INVOKE; 													=Toggle Switch
