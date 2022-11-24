using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Manager_Announcements : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_TemplateAnnouncement;
    [SerializeField] private GameObject par_Announcements;
    [SerializeField] private Transform pos_FirstAnnouncement;

    //public but hidden variables
    [HideInInspector] public List<GameObject> Announcements = new();

    //create a new announcement with specified text,
    //delete the oldest created one if total count is over 5
    public void CreateAnnouncement(string message)
    {
        GameObject newAnnouncement =  Instantiate(par_TemplateAnnouncement,
                                                  par_Announcements.transform.position,
                                                  Quaternion.identity,
                                                  par_Announcements.transform);
        TMP_Text txt_AnnouncementMessage = newAnnouncement.GetComponentInChildren<TMP_Text>();
        txt_AnnouncementMessage.text = message;

        newAnnouncement.AddComponent<UI_Announcement>();
        newAnnouncement.GetComponent<UI_Announcement>().isActivated = true;

        for (int i = 0; i < Announcements.Count; i++)
        {
            GameObject ann = Announcements[i];
            if (ann == null)
            {
                Announcements.Remove(ann);
            }
        }
        if (Announcements.Count > 0)
        {
            if (Announcements.Count > 4)
            {
                Destroy(Announcements[0]);
            }

            foreach (GameObject announcement in Announcements)
            {
                if (announcement != null)
                {
                    announcement.transform.position = new Vector3(announcement.transform.position.x,
                                                                  announcement.transform.position.y - 60,
                                                                  announcement.transform.position.z);
                }
            }
        }

        Announcements.Add(newAnnouncement);
        newAnnouncement.transform.position = pos_FirstAnnouncement.position;
    }
}