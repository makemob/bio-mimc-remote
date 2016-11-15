using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;


// status packet recieved from server to indicate the actual state of the UKI
// (might have been set by a different remote client)
public struct UKIStatus
{
	public uint netId;     // netId of client that last changed UKIStatus
	public int legMode;
	public float legSpeed;
	public int wingMode;
	public float wingSpeed;
}


public class ClientNetworkManager : MonoBehaviour
{
	public static ClientNetworkManager Instance { get; private set; }

	public NetworkManager m_networkManager;
	public Text m_statusText;
	public Text m_debugText;
	public InputField m_addressInput;

	public Toggle m_legMode0;
	public Toggle m_legMode1;
	public Toggle m_legMode2;
	public Toggle m_legMode3;
	public Toggle m_wingMode0;
	public Toggle m_wingMode1;
	public Toggle m_wingMode2;
	public Toggle m_wingMode3;
	public Toggle m_wingMode4;

	private UKIStatus m_status;
	private bool m_lastConnected = true;
	private Connection m_connection;
	private float m_offlineTime;
	private float m_timeSinceLastContact;


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
	public void UISetLegMode(Toggle toggle)
	{
		if (toggle.isOn)
		{
			int mode = int.Parse(Regex.Match(toggle.name, @"\d+").Value);
			//Debug.Log("UISetLegMode " + mode);
			if (m_connection != null)
				m_connection.CmdSetUKILegMode(mode);
		}
	}

	// callback for UI
	public void UISetLegSpeed(Slider slider)
	{
		float speed = slider.value;	// 0.0 -> 1.0
		//Debug.Log("UISetLegSpeed " + speed);
		if (m_connection != null)
			m_connection.CmdSetUKILegSpeed(speed);
	}

	// callback for UI
	public void UISetWingMode(Toggle toggle)
	{
		if (toggle.isOn)
		{
			int mode = int.Parse(Regex.Match(toggle.name, @"\d+").Value);
			//Debug.Log("UISetWingMode " + mode);
			if (m_connection != null)
				m_connection.CmdSetUKIWingMode(mode);
		}
	}

	// callback for UI
	public void UISetWingSpeed(Slider slider)
	{
		float speed = slider.value; // 0.0 -> 1.0
		//Debug.Log("UISetWingSpeed " + speed);
		if (m_connection != null)
			m_connection.CmdSetUKIWingSpeed(speed);
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


	private static int count;
	private static Toggle.ToggleEvent emptyToggleEvent = new Toggle.ToggleEvent();
	//private static Slider.SliderEvent emptySliderEvent = new Slider.SliderEvent();

	public void OnReceiveStatusFromServer(UKIStatus status)
	{
		int thisNetId = m_connection != null ? (int)m_connection.netId.Value : -1;

		m_debugText.text = string.Format(
			"this netId: {0}\n" +
			"packets from server: {1}\n" +
			//"UKI status: netId={2} legMode={3} legSpeed={4:0.00} wingMode={5} wingSpeed={6:0.00}",
			"UKI status: netId={2} legMode={3} wingMode={5}",
			thisNetId,
			++count,
			status.netId,
			status.legMode,
			status.legSpeed,
			status.wingMode,
			status.wingSpeed);

		if (status.netId != thisNetId)
		{
			// UKI status has been set by a different client
			// - adjust local settings to reflect actual status of UKI

			Toggle toggle = null;
			switch (status.legMode)
			{
				case 0: toggle = m_legMode0; break;
				case 1: toggle = m_legMode1; break;
				case 2: toggle = m_legMode2; break;
				case 3: toggle = m_legMode3; break;
			}

			if (toggle != null)
			{
				var originalEvent = toggle.onValueChanged;
				toggle.onValueChanged = emptyToggleEvent;
				toggle.isOn = true;
				toggle.onValueChanged = originalEvent;
			}

			toggle = null;
			switch (status.wingMode)
			{
				case 0: toggle = m_wingMode0; break;
				case 1: toggle = m_wingMode1; break;
				case 2: toggle = m_wingMode2; break;
				case 3: toggle = m_wingMode3; break;
				case 4: toggle = m_wingMode4; break;
			}

			if (toggle != null)
			{
				var originalEvent = toggle.onValueChanged;
				toggle.onValueChanged = emptyToggleEvent;
				toggle.isOn = true;
				toggle.onValueChanged = originalEvent;
			}
		}

		m_timeSinceLastContact = 0.0f;
	}


	private void Reconnect()
	{
		// stop existing connection / stop trying to connect
		m_networkManager.StopClient();

		// trigger a new connection attempt
		m_lastConnected = true;

		// reset offline timer
		m_offlineTime = 0.0f;
	}


	private void Update()
	{
		bool isConnected = m_networkManager.IsClientConnected();

		if (isConnected)
		{
			if ((m_timeSinceLastContact += Time.unscaledDeltaTime) >= 5.0f)
			{
				// network manager thinks it's connected to server but no packets are being received - try reconnecting
				Reconnect();
				isConnected = false;
			}

			// reset timer
			m_offlineTime = 0.0f;
		}
		else if ((m_offlineTime += Time.unscaledDeltaTime) > 5.0f)
		{
			// offline for more than 5 seconds - try reconnecting
			Reconnect();
		}

		// maintain a connection to the server
		if (m_lastConnected && !isConnected)
		{
			// startup or lost connection
			string text = string.Format("<color=#FF4747FF>Offline.</color>\nTrying to connect to {0}:{1}...",
				m_networkManager.networkAddress, m_networkManager.networkPort);
			Debug.Log(text);
			m_statusText.text = text;

			m_networkManager.StartClient();
		}
		else if (!m_lastConnected && isConnected)
		{
			string text = string.Format("<color=#00FF00FF>Online.</color>\nConnected to {0}:{1}",
				m_networkManager.networkAddress, m_networkManager.networkPort);
			Debug.Log(text);
			m_statusText.text = text;
		}

		m_lastConnected = isConnected;


		if (Input.GetKeyDown(KeyCode.Escape))
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}

