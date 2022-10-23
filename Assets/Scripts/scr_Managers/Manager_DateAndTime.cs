using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_DateAndTime : MonoBehaviour
{
    [Header("Assignables")]
    [Range(0.1f, 10000)]
    [SerializeField] private float timeSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject pos_SunLookAt;
    [SerializeField] private GameObject pos_RotationCenter;

    //day and month names
    private readonly string[] Days = new string[]
    {
        "Morndas", "Tirdas", "Middas", "Turdas", "Fredas", "Loredas", "Sundas"
    };
    private readonly Dictionary<string, int> Months = new();

    //time and date values
    private string fullTime;
    [HideInInspector] public int daysSinceLastRestart = 3;
    [HideInInspector] public int minute;
    [HideInInspector] public int hour;
    [HideInInspector] public string dayName;
    [HideInInspector] public string monthName;

    //private variables
    private bool moveSun;
    private float clockTimer = 2;
    private float sunCountdownTimer;
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

        //apply default date
        SetDateAndTime(0,
                       12,
                       "27 Morndas",
                       "Last Seed");

        sun.transform.LookAt(pos_SunLookAt.transform.position);
        
        LocationsScript = GetComponent<Manager_Locations>();
    }

    private void Update()
    {
        if (moveSun)
        {
            sunCountdownTimer -= Time.deltaTime;

            float step = rotationSpeed * Time.deltaTime;
            pos_RotationCenter.transform.Rotate(new Vector3(0, 0, 1) * step);
            sun.transform.LookAt(pos_SunLookAt.transform.position);

            if (sunCountdownTimer <= 0)
            {
                moveSun = false;
            }
        }

        clockTimer -= Time.deltaTime * timeSpeed;
        if (clockTimer <= 0)
        {
            minute++;

            if (minute >= 60)
            {
                SetSunPosition();

                hour++;

                if (hour >= 24)
                {
                    UpdateDayAndMonth();

                    hour = 0;
                }

                minute = 0;
            }

            clockTimer = 2;
        }

        fullTime = minute + ":" + hour + ", " + dayName + " of " + monthName + " in the 3rd era 433";
    }

    //update sun position after every 30 in-game minutes
    public void SetSunPosition()
    {
        sunCountdownTimer = 1;

        moveSun = true;
    }

    //apply a date and time that the game starts counting from the launch of the game
    public void SetDateAndTime(int min, int hr, string dateNumberAndDay, string month)
    {
        minute = min;
        hour = hr;
        dayName = dateNumberAndDay;
        monthName = month;
    }

    private void UpdateDayAndMonth()
    {
        daysSinceLastRestart--;
        if (daysSinceLastRestart == 0)
        {
            LocationsScript.ResetAllLocations();
        }

        string[] dayNameSplit = dayName.Split(" ");
        int dayDate = int.Parse(dayNameSplit[0]);
        int newDayDate = dayDate++;

        string theDayName = dayNameSplit[1];

        int currentDayIndex = Array.IndexOf(Days, theDayName);
        int newDayIndex = currentDayIndex++;
        if (newDayIndex > 6)
        {
            newDayIndex = 0;
        }
        Debug.Log("Trying to set " + newDayIndex + " as new day index...");
        dayName = newDayDate + " " + Days[newDayIndex];

        List<string> monthNames = new();
        foreach (KeyValuePair<string, int> months in Months)
        {
            monthNames.Add(months.Key);
        }
        int monthIndex = monthNames.IndexOf(monthName);
        int newMonthIndex = monthIndex++;
        if (newMonthIndex > 11)
        {
            newMonthIndex = 0;
        }
        string newMonthName = monthNames[newMonthIndex];

        foreach (KeyValuePair<string, int> months in Months)
        {
            string month = months.Key;
            int daysInMonth = months.Value;

            if (month == monthName
                && daysInMonth > newDayIndex)
            {
                dayName = "1 " + newMonthName;
            }
        }

        Debug.Log(fullTime);
    }
}