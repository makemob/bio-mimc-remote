using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;


// status packet recieved from server to indicate the actual state of the UKI
// (might have been set by a different remote client)
public struct UKIStatus
{
	public uint netId;     // netId of client that last changed UKIStatus
	public int mode;
	public float speed;
}


public class ClientNetworkManager : MonoBehaviour
{
	public static ClientNetworkManager Instance { get; private set; }

	public NetworkManager m_networkManager;
	public Text m_statusText;
	public InputField m_addressInput;

	private UKIStatus m_status;
	private bool m_lastConnected = true;
	private Connection m_connection;
	private float m_offlineTime;


	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	private void Start()
	{
		// initialise server address
		m_networkManager.networkAddress = PlayerPrefs.GetString("ServerAddress", "localhost");
		m_addressInput.text = m_networkManager.networkAddress;
	}


	public void StartLocalConnection(Connection connection)
	{
		m_connection = connection;
	}

	public void StopLocalConnection(Connection connection)
	{
		m_connection = null;
	}


	// callback for UI
	public void UISetMode(Toggle toggle)
	{
		if (toggle.isOn)
		{
			int mode = int.Parse(Regex.Match(toggle.name, @"\d+").Value);
			Debug.Log("UISetMode " + mode);
			if (m_connection != null)
				m_connection.CmdSetUKIMode(mode);
		}
	}

	// callback for UI
	public void UISetSpeed(Slider slider)
	{
		float speed = slider.value;	// 0.0 -> 1.0
		Debug.Log("UISetSpeed " + speed);
		if (m_connection != null)
			m_connection.CmdSetUKISpeed(speed);
	}

	// callback for UI
	public void UISetServerAddress(InputField input)
	{
		Debug.Log("UISetServerAddress " + input.text);

		// disconnect or stop trying to connect
		m_networkManager.StopClient();

		// set new address
		m_networkManager.networkAddress = input.text;
		PlayerPrefs.SetString("ServerAddress", m_networkManager.networkAddress);
		PlayerPrefs.Save();

		// trigger a new connection attempt
		m_lastConnected = true;
	}

	private void Update()
	{
		bool isConnected = m_networkManager.IsClientConnected();

		if (isConnected)
		{
			// reset timer
			m_offlineTime = 0.0f;
		}
		else if ((m_offlineTime += Time.deltaTime) > 5.0f)	// if offline for more than 5 seconds
		{
			Debug.Log("timeout");

			// stop trying to connect
			m_networkManager.StopClient();

			// trigger a new connection attempt
			m_lastConnected = true;

			m_offlineTime = 0.0f;
		}

		// maintain a connection to the server
		if (m_lastConnected && !isConnected)
		{
			// startup or lost connection
			string text = string.Format("<color=#FF4747FF>Offline.</color>\nTrying to connect to {0}:{1}...", m_networkManager.networkAddress, m_networkManager.networkPort);
			Debug.Log(text);
			m_statusText.text = text;

			m_networkManager.StartClient();
		}
		else if (!m_lastConnected && isConnected)
		{
			string text = string.Format("<color=#00FF00FF>Online.</color>\nConnected to {0}:{1}", m_networkManager.networkAddress, m_networkManager.networkPort);
			Debug.Log(text);
			m_statusText.text = text;
		}

		m_lastConnected = isConnected;
	}
}

