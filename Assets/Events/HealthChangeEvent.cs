using UnityEngine;

[CreateAssetMenu(fileName = "HealthChangeEvent", menuName = "Scriptable Objects/Health Change Event")]
public class HealthChangeEvent : GenericEvent<HealthManager.EventData> { }
