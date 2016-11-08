using UnityEngine;
using UnityEngine.Networking;

// This is the player gameobject representing a connection to the server.
// This class is instantiated on both the client side and server side.
// Commands only run on the server-side instance of this NetworkBehaviour.
// (No command code required on this client-side instance.)

public class Connection : NetworkBehaviour
{
	[SyncVar]
	public int m_fromServer;

	[Command]
	public void CmdSetUKIMode(int mode) { }
	[Command]
	public void CmdSetUKISpeed(float speed) { }


	private void Start()
	{
		if (ClientNetworkManager.Instance != null && isLocalPlayer)
			ClientNetworkManager.Instance.StartLocalConnection(this);
	}

	private void OnDestroy()
	{
		if (ClientNetworkManager.Instance != null && isLocalPlayer)
			ClientNetworkManager.Instance.StopLocalConnection(this);
	}
}
