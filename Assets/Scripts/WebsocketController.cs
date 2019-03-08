using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebsocketController : MonoBehaviour
{

	public string Key;
	public float Speed = 0.5f;
	private ClientWebSocket webSocket = null;

    private const int receiveChunkSize = 256;

	private Sensor sensor;

    /// <summary>
	/// Walkmode object is needed to set speed and direction of movement
	/// </summary>
	private WalkMode currentWalkMode = 0;

	/// <summary>
	/// TurnMode object is needed to set intensity of lateral movement
	/// </summary>
	private TurnMode currentTurnMode = 0;
	
	/// <summary>
	/// robot can move forwards with three different velocities, stop and move backwards
	/// </summary>
	public enum WalkMode
	{
		BACKWARDS = -1,
		STOP = 0,
		FORWARDS_SLOW = 1,
		FORWARDS_MEDIUM = 2,
		FORWARDS_FAST = 3
	}

	/// <summary>
	/// robot can turn left and right in two intensities or move straight
	/// </summary>
	public enum TurnMode
	{
		LEFT_HARD = -2,
		LEFT_SMOOTH = -1,
		STRAIGHT = 0,
		RIGHT_SMOOTH = 1,
		RIGHT_HARD = 2
	}

	private async void ConnectToSocket()
	{
		webSocket = new ClientWebSocket();
        var builder = new UriBuilder
        {
            Host = "api.zyrus.space",
            Port = 80,
            Scheme = "ws",
            Path = "/socket"
        };
        Debug.Log(builder.Uri);
		await webSocket.ConnectAsync(builder.Uri, CancellationToken.None);
		Debug.Log(webSocket.State.ToString());
		Send();
		await Receive();
	}
	
	// Use this for initialization
	void Start ()
	{
		sensor = transform.Find("Sensor").GetComponent<Sensor>();
		ConnectToSocket();
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate((int)currentWalkMode * Vector3.forward * Speed * Time.deltaTime);
		transform.Rotate(Vector3.up, (int)currentTurnMode * Time.deltaTime * 25);
	}
	
	public async Task Send()
	{
		while (webSocket.State == WebSocketState.Open)
		{
			var message = new StringBuilder();

			message.Append(Key).Append("/");
			message.Append((int) currentWalkMode).Append("/");
			message.Append((int) currentTurnMode).Append("/");
			message.Append(string.Join(",", sensor.list));

			var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString()));
			await webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}
	
	private async Task Receive()
	{
		

		while (webSocket.State == WebSocketState.Open)
		{
			var buffer = new byte[receiveChunkSize];
			var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			if (response.MessageType == WebSocketMessageType.Close)
			{
				await
					webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close response received",
						CancellationToken.None);
			}
			else
			{
				var result = Encoding.UTF8.GetString(buffer).TrimEnd((char)0).Split('/');

				var key = result[0];
				var action = result[1];

				if (key.Equals(Key))
				{
					if (action.Equals("forward") && currentWalkMode < WalkMode.FORWARDS_FAST)
					{
						currentWalkMode++;
					}
					if (action.Equals("backward") && currentWalkMode > WalkMode.BACKWARDS)
					{
						currentWalkMode--;
					}
					if (action.Equals("left") && currentTurnMode > TurnMode.LEFT_HARD)
					{
						currentTurnMode--;
					}
					if (action.Equals("right") && currentTurnMode < TurnMode.RIGHT_HARD)
					{
						currentTurnMode++;
					}
					if (action.Equals("toggle"))
					{
						sensor.ToggleSensorRays();
					}
				    if (action.Equals("walk"))
				    {
				        var walk = int.Parse(result[2]);
				        if (walk >= -1 && walk <= 3)
                            currentWalkMode = (WalkMode) walk;
				    }
				    if (action.Equals("turn"))
				    {
				        var turn = int.Parse(result[2]);
                        if (turn >= -2 && turn <= 2)
				            currentTurnMode = (TurnMode) turn;
				    }
                }
				
				
				Debug.Log("\"" + action + "\"");
			}
		}
	}

	private void OnDestroy()
	{
		webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close response received",
			CancellationToken.None);
	}
}
