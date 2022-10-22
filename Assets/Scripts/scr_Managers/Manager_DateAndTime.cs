using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_DateAndTime : MonoBehaviour
{
    //day and month names
    private readonly string[] Days = new string[]
    {
        "Morndas", "Tirdas", "Middas", "Turdas", "Fredas", "Loredas", "Sundas"
    };
    private readonly Dictionary<string, int> Months = new();

    //time and date values
    private string fullTime;
    [HideInInspector] public int daysSinceLastRestart = 3;
    [HideInInspector] public float second;
    [HideInInspector] public int minute;
    [HideInInspector] public int hour;
    [HideInInspector] public string dayName;
    [HideInInspector] public string monthName;

    //private variables
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
                       0,
                       12,
                       "27 Morndas",
                       "Last Seed");

        LocationsScript = GetComponent<Manager_Locations>();
    }

    //apply a date and time that the game starts counting from the launch of the game
    public void SetDateAndTime(float sec, int min, int hr, string dateNumberAndDay, string month)
    {
        second = sec;
        minute = min;
        hour = hr;
        dayName = dateNumberAndDay;
        monthName = month;
    }

    private void Update()
    {
        second += Time.deltaTime * 30;
        if (second >= 60)
        {
            minute++;

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

            second = 0;
        }

        fullTime = Mathf.Floor(second) + ":" + minute + ":" + hour + ", " + dayName + " of " + monthName + " in the 3rd era 433";
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