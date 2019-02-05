#ACTION KEYFRAMES GUIDE



'directorname = the name of the director you want to affect
'keyname = the name of the key frame or action key frame you want to affect
'groupname = the name of the container you want to affect
'tagname = the name of a tag/stop point tou want to affect
'scriptsub = the name of a subroutine in a script
'; = semi-colons are used to end one script and move to another. if your script is only one line(action) then it is not needed.

THIS_DIRECTOR		= Works As Exactly what you think


THIS_SCENE*STAGE*DIRECTOR*directorname START;                                	= Starts Director(directorname)

THIS_SCENE*STAGE*DIRECTOR*directorname SHOW F20;     							= Go to Frame 20 in Timeline

THIS_SCENE*STAGE*DIRECTOR*directorname CONTINUE;	                  			= Continue from this point in Time.

THIS_SCENE*STAGE*DIRECTOR*directorname CONTINUE REVERSE;                     	= Continues in reverse from this point in Time.
THIS_SCENE*STAGE*DIRECTOR*directorname*ACTION*A*KEY*$keyname*TIME SET F20;  	= Sets Action keyframe(keyname) to frame 20.

THIS_SCENE*STAGE*DIRECTOR*directorname SHOW $tagname;                  			= Go to Stop point "tagname" 



RENDERER*TREE*$groupname*TRANSFORMATION*POSITION*Y SET 45.0;            	= Set Position of Container(groupname)

RENDERER*TREE*$groupname*ACTIVE SET 0;                                  	= Set Container(groupname) On/Off

RENDERER*TREE*$groupname*FUNCTION*Omo*vis_con SET 0;	                	= Set Omo Selector

RENDERER*STAGE*DIRECTOR*groupname*KEY*$keyname*TIME SET F5;            		= change stop point time frame

RENDERER*TREE*$groupname*FUNCTION*Grid*num_row SET 5;                   	= change the row set in a grid arrange

RENDERER*TREE*$groupname*GEOM*#608411*height SET 200.0	                	= change the height of the noggi through animation 
*find geom # name through console
*(This number changes all the time: vizid).

RENDERER*TREE*$groupname*FUNCTION*Maxsize*WIDTH_X SET 800.0;				= Change maxsize width

RENDERER*TREE*$groupname*FUNCTION*Maxsize*DEFAULT_SCALE_Y SET .4			= Change maxsize Y scale

THIS_SCENE*GFX*1*STAGE START												= Starts graphic channel 1


TRANSFORMATION*POSITION*Y
FUNCTION*Grid*num_row
GEOM*height



RENDERER*TREE*$groupname*FUNCTION*Grid*num_row SET 1; 
RENDERER*TREE*$groupname*TRANSFORMATION*POSITION*Y SET 50; 
RENDERER*TREE*$groupname*ANIMATION*KEY*$keyname*VALUE SET 64;
THIS_SCENE*TREE*$groupname*SCRIPT INVOKE scriptsub;							= invokes subroutine in the script






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
