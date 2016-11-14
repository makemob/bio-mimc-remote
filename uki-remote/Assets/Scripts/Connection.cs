using UnityEngine;
using UnityEngine.Networking;


// This is the player gameobject representing a connection to the server.
// This class is instantiated on both the client and server.
// [Command] methods execute code on the server-side instance only.
// [ClientRpc] methods execute code on the client-side instance only.


public class Connection : NetworkBehaviour
{
	//[SyncVar]
	//public int m_fromServer;


	// commands (client -> server) - code executed on server-side only
	[Command]
	public void CmdSetUKILegMode(int mode) { }
	[Command]
	public void CmdSetUKIWingMode(int mode) { }
	[Command]
	public void CmdSetUKILegSpeed(float speed) { }
	[Command]
	public void CmdSetUKIWingSpeed(float speed) { }


	// client rpcs (server -> client) - code executed on client-side only
	[ClientRpc]
	private void RpcRefreshStatus(UKIStatus status)
	{
		if (ClientNetworkManager.Instance != null && isLocalPlayer)
			ClientNetworkManager.Instance.OnReceiveStatusFromServer(status);
	}


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
