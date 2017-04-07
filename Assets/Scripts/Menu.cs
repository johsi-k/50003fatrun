using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    public Canvas MainCanvas;
    public Canvas OptionsCanvas;
    // public Canvas SelectModeCanvas;
    //public Canvas FriendsModeCanvas;
    public Canvas CharacterSelectCanvas;
    public Canvas WaitingRoomCanvas;
    public Canvas MainCanvasWithHelpCanvas;
    public Canvas ChoosePartyModeCanvas;
    public Canvas AddPeopleCanvas;
    public Canvas JoinPartyCanvas;
    public Canvas LoadingCanvas;

    void Awake()
    {
        OptionsCanvas.enabled = false;
        //    SelectModeCanvas.enabled = false;
        //   FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void OptionsOn()
    {
        OptionsCanvas.enabled = true;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = false;
        // FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void StartOn()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        // SelectModeCanvas.enabled = false;
        // FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = true;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void ReturnOn()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = true;
        // SelectModeCanvas.enabled = false;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }

    public void RandomMatchWaitingRoom()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = true;
        MainCanvasWithHelpCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void Help()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = true;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void closeHelp()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        MainCanvas.enabled = true;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }
    public void choosePartyMode()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        MainCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = true;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }


    public void createParty()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        MainCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = true;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = false;

    }

    public void joinParty()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        MainCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = true;
        LoadingCanvas.enabled = false;

    }

    public void loading()
    {
        OptionsCanvas.enabled = false;
        MainCanvas.enabled = false;
        //  SelectModeCanvas.enabled = true;
        //  FriendsModeCanvas.enabled = false;
        CharacterSelectCanvas.enabled = false;
        WaitingRoomCanvas.enabled = false;
        MainCanvasWithHelpCanvas.enabled = false;
        MainCanvas.enabled = false;
        ChoosePartyModeCanvas.enabled = false;
        AddPeopleCanvas.enabled = false;
        JoinPartyCanvas.enabled = false;
        LoadingCanvas.enabled = true;
    }


    public void LoadOn()
    {
        SceneManager.LoadScene("ui2");
    }
}
