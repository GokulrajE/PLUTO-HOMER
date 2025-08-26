using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class welcomSceneHandler : MonoBehaviour
{
    //public GameObject loading;
    public TextMeshProUGUI userName;
    public TextMeshProUGUI timeRemainingToday;
    public TextMeshProUGUI todaysDay;
    public TextMeshProUGUI todaysDate;
    public int daysPassed;
    public TextMeshProUGUI[] prevDays = new TextMeshProUGUI[7];
    public TextMeshProUGUI[] prevDates = new TextMeshProUGUI[7];
    public Image[] pies = new Image[7];
    public bool piChartUpdated = false; 
    private DaySummary[] daySummaries;
    public readonly string nextScene = "CHMECH";

    // Private variables
    private bool attachPlutoButtonEvent = false;
    bool changeScene = false;

    void Start()
    {
        if (string.IsNullOrEmpty(DataManager.basePath))
        {
            Debug.Log(DataManager.userIdPath);
            Debug.Log("basePath: " + DataManager.basePath);
Debug.Log("userID: " + AppData.Instance.userID);
Debug.Log("configFile path: " + DataManager.configFile);

            DataManager.basePath = DataManager.FixPath(Path.Combine(DataManager.userIdPath, "data"));
            Debug.LogWarning("basePath was empty. Setting fallback path: " + DataManager.basePath);
        }


        Debug.Log("path:" + DataManager.basePath);
        if (!Directory.Exists(DataManager.basePath))
        {
            SceneManager.LoadScene("CONFIG");
            return;
        }
          string filePath = @"C:/comport.txt";

        // Optional: Create a default file if it doesn't exist
        if (File.Exists(filePath))
        {
            string com = File.ReadAllText(filePath);
            AppData.Instance.setComport(com);
        }

        // Get all subdirectories excluding metadata
        var validUserDirs = Directory.GetDirectories(DataManager.basePath)
        .Select(Path.GetFileName)
        .Where(name => !name.ToLower().Contains("meta"))
        .ToList();


        if (validUserDirs.Count == 1) 
        {
            AppData.Instance.setUser(validUserDirs[0]);
            DataManager.setUserId(AppData.Instance.userID);
        }

        if (!File.Exists(DataManager.configFile)) 
        {
            Debug.Log("running");
            SceneManager.LoadScene("CONFIG");
            return;
        }
        
        // Initialize.
        AppData.Instance.Initialize(SceneManager.GetActiveScene().name);
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"'{SceneManager.GetActiveScene().name}' scene started.");
        daySummaries = AppData.Instance.userData.CalculateMoveTimePerDay();
        
        // Update summary display
        if (!piChartUpdated)
        {
            UpdateUserData();
            UpdatePieChart();
        }
        Task.Run(() =>  // Run in a background task
            {
            if (!awsManager.IsTaskScheduled(awsManager.taskName))
            {
                awsManager.ScheduleTask();
            }
            awsManager.RunAWSpythonScript();

            });
       
    }

    void Update()
    {
        if (!attachPlutoButtonEvent && Time.timeSinceLevelLoad > 1)
        {
            attachPlutoButtonEvent = true;
            PlutoComm.OnButtonReleased += onPlutoButtonReleased;
        }
        // Check if it time to switch to the next scene
        if (changeScene == true ) {
            LoadTargetScene();
            changeScene = false;
        }
    }

    public void onPlutoButtonReleased()
    {
        AppLogger.LogInfo("PLUTO button released.");
        changeScene = true;
    }

    private void LoadTargetScene()
    {
        AppLogger.LogInfo($"Switching to the next scene '{nextScene}'.");
        SceneManager.LoadScene(nextScene);
    } 


    private void UpdateUserData()
    {
        userName.text = AppData.Instance.userData.hospNumber;
        timeRemainingToday.text = $"{AppData.Instance.userData.totalMoveTimeRemaining} min";
        todaysDay.text = AppData.Instance.userData.getCurrentDayOfTraining().ToString();
        todaysDate.text = DateTime.Now.ToString("ddd, dd-MM-yyyy");
        if (!File.Exists(awsManager.filePathUploadStatus))
            awsManager.createFile(userName.text);
        
    }

    private void UpdatePieChart()
    {
        int N = daySummaries.Length;  
        for (int i = 0; i < N; i++)
        {
            Debug.Log($"{i} | {daySummaries[i].Day} | {daySummaries[i].Date} | {daySummaries[i].MoveTime}");
            prevDays[i].text = daySummaries[i].Day;
            prevDates[i].text = daySummaries[i].Date;
            pies[i].fillAmount = daySummaries[i].MoveTime / AppData.Instance.userData.totalMoveTimePrsc;
            pies[i].color = new Color32(148,234,107,255);
        }
        piChartUpdated = true;
    }

    private void OnDestroy()
    {
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased -= onPlutoButtonReleased;
        }
    }

    private void OnApplicationQuit()
    {
        ConnectToRobot.disconnect();
        AppLogger.StopLogging();
    }
}