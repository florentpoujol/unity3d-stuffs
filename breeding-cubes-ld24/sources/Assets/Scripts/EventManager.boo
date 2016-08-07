import UnityEngine


class EventManager: 

	// cube is Hovered 
	/*public static event cubeIsHoveredMethods as callable (GameObject);

	public static def OnCubeIsHovered (cube):
		cubeIsHoveredMethods (cube) if cubeIsHoveredMethods != null */


	// left mouse button is clicked
	public static event applicationReloadMethods as callable ()

	public static def OnApplicationReload ():
		applicationReloadMethods () if applicationReloadMethods != null


	// player touch a cube he can mate with
	public static event playerTouchACubeMethods as callable (GameObject)

	public static def OnPlayerTouchACube (cube):
		playerTouchACubeMethods (cube) if playerTouchACubeMethods != null


	// player procreate with another cube
	public static event playerProcreateMethods as callable (GameObject)

	public static def OnPlayerProcreate (cube):
		playerProcreateMethods (cube) if playerProcreateMethods != null


	// player gets killed
	public static event playerKilledMethods as callable ()

	public static def OnPlayerKilled ():
		playerKilledMethods () if playerKilledMethods != null