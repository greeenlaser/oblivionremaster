using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_DateAndTime : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject par_RotationCenter;
    [SerializeField] private GameObject par_SunLookAt;

    //public but hidden variables
    [HideInInspector] public int daysSinceLastRestart = 3;
    [HideInInspector] public int minute;
    [HideInInspector] public int hour;
    [HideInInspector] public string dayName;
    [HideInInspector] public string monthName;

    //day and month names
    public readonly string[] Days = new string[]
    {
        "Morndas", "Tirdas", "Middas", "Turdas", "Fredas", "Loredas", "Sundas"
    };
    public readonly Dictionary<string, int> Months = new();

    //private variables
    private bool moveSun;
    private float sunCountdownTimer;
    private int tenMinuteCounter = 10;
    private float clockTimer = 2;
    private string fullTime;
    private Manager_Locations LocationsScript;

    private void Awake()
    {
        Months["Morning Star"] = 31;
        Months["Sun's Dawn"] = 28;
        Months["First Seed"] = 31;
        Months["Rain's Hand"] = 30;
        Months["Second Seed"] = 31;
        Months["Midyear"] = 30;
        Months["Sun's Height"] = 31;
        Months["Last Seed"] = 31;
        Months["Hearthfire"] = 30;
        Months["Frostfall"] = 31;
        Months["Sun's Dusk"] = 30;
        Months["Evening Star"] = 31;

        LocationsScript = GetComponent<Manager_Locations>();
    }

    private void Update()
    {
        if (moveSun)
        {
            sunCountdownTimer -= Time.deltaTime;

            float step = 2.5f * Time.deltaTime;
            par_RotationCenter.transform.Rotate(new Vector3(1, 0, 0) * step);
            sun.transform.LookAt(par_SunLookAt.transform);

            if (sunCountdownTimer <= 0)
            {
                moveSun = false;
            }
        }

        clockTimer -= Time.deltaTime;
        if (clockTimer <= 0)
        {
            minute++;
            tenMinuteCounter--;
            if (tenMinuteCounter == 0)
            {
                Debug.Log("Info: Time is " + hour + ":00.");
                SetSunPosition();
            }

            if (minute >= 60)
            {
                hour++;

                if (hour >= 24)
                {
                    UpdateDayAndMonth();

                    hour = 0;
                }

                minute = 0;
            }

            fullTime = minute + ":" + hour + ", " + dayName + " of " + monthName + " in the 3rd era 433";

            clockTimer = 2;
        }
    }

    //update sun position after every 60 in-game minutes
    private void SetSunPosition()
    {
        tenMinuteCounter = 10;
        sunCountdownTimer = 1;
        moveSun = true;
    }

    //apply a date and time that the game starts counting from the launch of the game
    public void SetDateAndTime(int min, 
                               int hr, 
                               string dateNumberAndDay, 
                               string month)
    {
        minute = min;
        hour = hr;
        dayName = dateNumberAndDay;
        monthName = month;

        bool continueCounting = false;
        float x = 0;
        int total = 0;
        if (hour == 0)
        {
            if (minute == 0)
            {
                StartCoroutine(SetSunRotationAtStart(x));
            }
            else
            {
                continueCounting = true;
                total = minute;
            }
        }
        else
        {
            continueCounting = true;
            total = minute + (hour * 60);
        }

        if (continueCounting)
        {
            int count = total / 20;
            x = 2.5f * count;

            StartCoroutine(SetSunRotationAtStart(x));
        }

        fullTime = minute + ":" + hour + ", " + dayName + " of " + monthName + " in the 3rd era 433";
    }

    //updates day name, day number and month
    private void UpdateDayAndMonth()
    {
        daysSinceLastRestart--;
        if (daysSinceLastRestart == 0)
        {
            LocationsScript.ResetAllLocations();
        }

        string[] dayNameSplit = dayName.Split(" ");

        int dayDate = int.Parse(dayNameSplit[0]);
        dayDate++;

        string theDayName = dayNameSplit[1];

        int currentDayIndex = Array.IndexOf(Days, theDayName);
        currentDayIndex++;
        if (currentDayIndex > 6)
        {
            currentDayIndex = 0;
        }

        dayName = dayDate + " " + Days[currentDayIndex];

        List<string> monthNames = new();
        foreach (KeyValuePair<string, int> months in Months)
        {
            monthNames.Add(months.Key);
        }

        int monthIndex = monthNames.IndexOf(monthName);
        foreach (string monthName in monthNames)
        {
            if (fullTime.Contains(monthName)
                && dayDate > Months[monthName])
            {
                dayName = 1 + " " + Days[currentDayIndex];
                monthIndex++;
                if (monthIndex > 11)
                {
                    monthIndex = 0;
                }
                break;
            }
        }
        monthName = monthNames[monthIndex];

        fullTime = minute + ":" + hour + ", " + dayName + " of " + monthName + " in the 3rd era 433";
    }

    private IEnumerator SetSunRotationAtStart(float targetAngle)
    {
        yield return null;
        float x = 0;

        //rotates 2.5 degrees per turn until it reaches target angle
        while (x < targetAngle + 2.5f)
        {
            par_RotationCenter.transform.rotation = Quaternion.Euler(x, 0, -30);
            x += 2.5f;
        }

        sun.transform.LookAt(par_SunLookAt.transform);
    }
}