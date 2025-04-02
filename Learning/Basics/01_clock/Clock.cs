using System;
using UnityEngine;

public class Clock : MonoBehaviour{
    const float hours_to_degrees = -30f;
    const float minutes_to_degrees = -6f;
    const float seconds_to_degrees = -6f;
    [SerializeField]Transform hours_pivot, minutes_pivot, seconds_pivot;
    void Update(){
        TimeSpan time = DateTime.Now.TimeOfDay;
        hours_pivot.localRotation = Quaternion.Euler(0, 0, hours_to_degrees * (float)time.TotalHours);
        minutes_pivot.localRotation = Quaternion.Euler(0, 0, minutes_to_degrees * (float)time.TotalMinutes);
        seconds_pivot.localRotation = Quaternion.Euler(0, 0, seconds_to_degrees * (float)time.TotalSeconds);
    }
}