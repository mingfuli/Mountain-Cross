﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {
    public GameObject landscapeCanvas;
    public GameObject portraitCanvas;

    private GameObject rule;
    
    // public GameObject shop;
    private GameObject shop;

    private static GameObject playerMoveCount;

    private static GameObject player;

    private static GameObject sceneManager;

    AudioSource sceneAudio;
    public AudioClip changeSceneSound;

    public static int diceSideThrown = 0;

    public static bool gameOver = false;

    public static bool hasFinishedReward = true;
    public static bool isInStore = false;

    HashSet<int> minigamesIndexes = new HashSet<int>();
    private int[] waypoints_reward = {0, 0, 4, 0, 0, 0, 0,
                                      0, 0, 0, -10, 0, 0,
                                      0, 0, 0, 0, 0, -2,
                                      0, 0, 0, 0, 0, 0};
    private int[] score_rewad = {0, 0, 0, 0, 0, 0, 0,
                                0, -200, 0, 0, 0, 0,
                                0, 0, 0, 0, 340, 0,
                                0, -120, 0, 0, 0, 0};
    
    // Use this for initialization
    void Start () {
        ScreenRotate();

        minigamesIndexes.Add(1);
        minigamesIndexes.Add(4);
        minigamesIndexes.Add(5);
        minigamesIndexes.Add(9);
        minigamesIndexes.Add(10);
        minigamesIndexes.Add(12);
        minigamesIndexes.Add(15);
        minigamesIndexes.Add(19);
        minigamesIndexes.Add(21);

        playerMoveCount = GameObject.Find("PlayerMoveCount");
        player = GameObject.Find("Player");
        shop = GameObject.Find("Shop");
        sceneManager = GameObject.Find("SceneManager");
        sceneAudio = GetComponent<AudioSource>();

        player.GetComponent<PlayerMovement>().moveAllowed = false;

        playerMoveCount.gameObject.SetActive(false);

        // set shop to close initially
        shop.gameObject.SetActive(false);
        ItemControl.instance.refreshCurrentItems();

        // check if show game rules
        rule = GameObject.Find("Rule");
        rule.gameObject.SetActive(false);
        int isRuleShow = PlayerPrefs.GetInt("showGameRules", 0);
        if (isRuleShow == 1) {
            StartCoroutine(displayGameRules());
        } else if (isRuleShow == 0) {
            rule.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        ScreenRotate();

        // Check if the player finish the movement this round
        if (player.GetComponent<PlayerMovement>().moveFinished) {
            player.GetComponent<PlayerMovement>().moveAllowed = false;
            player.GetComponent<PlayerMovement>().currentWaypointIndex = player.GetComponent<PlayerMovement>().destinationWaypointIndex;
            PlayerPrefs.SetInt("lastWaypointIndex", player.GetComponent<PlayerMovement>().currentWaypointIndex);

            // Check for rewarded steps
            if (!hasFinishedReward) {
                rewardPlayer();
            }
        }

        // check if need to go to the ending scene
        GoToEndScene();
    }

    IEnumerator displayGameRules() {
        yield return new WaitForSeconds(0.7f);
        rule.gameObject.SetActive(true);
        StartCoroutine(closeGameRules());
    }

    IEnumerator closeGameRules() {
        yield return new WaitForSeconds(5f);
        rule.gameObject.SetActive(false);
        PlayerPrefs.SetInt("showGameRules", 0);
    }

    public void ScreenRotate() {
        if (landscapeCanvas.gameObject.activeSelf) {
            portraitCanvas.gameObject.SetActive(false);
        } else if (portraitCanvas.gameObject.activeSelf) {
            landscapeCanvas.gameObject.SetActive(false);
        }
        if (Application.platform != RuntimePlatform.Android) {
            landscapeCanvas.gameObject.SetActive(true);
            portraitCanvas.gameObject.SetActive(false);
        } else {
            if ((Screen.height / Screen.width > 1.99f || Screen.width / Screen.height > 1.99f) && (Application.platform == RuntimePlatform.Android)) {
                landscapeCanvas.gameObject.SetActive(true);
                portraitCanvas.gameObject.SetActive(false);
                if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown
                    || Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.FaceDown) {
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                }
            } else {
                if (Screen.width / Screen.height > 1.4f) {
                    if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight) {
                        landscapeCanvas.gameObject.SetActive(true);
                        portraitCanvas.gameObject.SetActive(false);
                    } else if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) {
                        landscapeCanvas.gameObject.SetActive(false);
                        portraitCanvas.gameObject.SetActive(true);
                    } else {
                        if (landscapeCanvas.gameObject.activeSelf) {
                            portraitCanvas.gameObject.SetActive(false);
                        } else if (portraitCanvas.gameObject.activeSelf) {
                            landscapeCanvas.gameObject.SetActive(false);
                        }
                    }
                } else {
                    landscapeCanvas.gameObject.SetActive(false);
                    portraitCanvas.gameObject.SetActive(true);
                }
            }
        }
    }

    public static void MovePlayer() {
        // Reset reward status
        hasFinishedReward = false;

        // Setup UI
        playerMoveCount.gameObject.SetActive(true);
        playerMoveCount.GetComponent<Text>().text = "Move " + diceSideThrown.ToString() + " steps";

        // Setup player movement
        player.GetComponent<PlayerMovement>().moveForward = true;
        player.GetComponent<PlayerMovement>().destinationWaypointIndex = player.GetComponent<PlayerMovement>().currentWaypointIndex + diceSideThrown;
        player.GetComponent<PlayerMovement>().moveFinished = false;
        player.GetComponent<PlayerMovement>().moveAllowed = true;
    }


      // move player through props     
      public static void MovePlayer(int steps) {
        // Reset reward status
        hasFinishedReward = false;

        // Setup UI
        playerMoveCount.gameObject.SetActive(true);
        playerMoveCount.GetComponent<Text>().text = "Move " + steps + " steps";

        // Setup player movement
        player.GetComponent<PlayerMovement>().moveForward = true;
        player.GetComponent<PlayerMovement>().destinationWaypointIndex = player.GetComponent<PlayerMovement>().currentWaypointIndex + steps;
        player.GetComponent<PlayerMovement>().moveFinished = false;
        player.GetComponent<PlayerMovement>().moveAllowed = true;
    }

    private void rewardPlayer() {
        sceneAudio.PlayOneShot(changeSceneSound);
        // Current Index
        int currentIndex = player.GetComponent<PlayerMovement>().currentWaypointIndex;

        // Check steps
        int rewardedSteps = waypoints_reward[currentIndex];

        // check and automic apply insurance
        bool isInsuranceApplicable = (PlayerStats.getItemNum(PlayerStats.insurance) > 0);
        // Set reward finish if no reward
        if (rewardedSteps == 0) {
            int currentGameScore = PlayerPrefs.GetInt("totalGameScore", 0);


            // Adjust gamescore if it is a rewarded score
            if (score_rewad[currentIndex] != 0) {
                // Show reward
                playerMoveCount.gameObject.SetActive(true);
                if (score_rewad[currentIndex] > 0) {
                    playerMoveCount.GetComponent<Text>().text = "Rewarded with $" + score_rewad[currentIndex];
                } else {

                    if (isInsuranceApplicable) {
                        playerMoveCount.GetComponent<Text>().text = "Your insurance saved you\nfrom penalty!";
                    }
                    else if (currentGameScore + score_rewad[currentIndex] <= 0) {
                        playerMoveCount.GetComponent<Text>().text = "Lose all the money...";
                    } else {
                        playerMoveCount.GetComponent<Text>().text = "Lose $" + Mathf.Abs(score_rewad[currentIndex]);
                    }
                }

                // Adjust score
                if (!isInsuranceApplicable) {
                    int newGameScore = Math.Max(currentGameScore + score_rewad[currentIndex], 0);
                    PlayerPrefs.SetInt("totalGameScore", newGameScore);
                } else {
                    PlayerStats.useOneItem(PlayerStats.insurance);
                }

                hasFinishedReward = true;
            } else {
                if (minigamesIndexes.Contains(currentIndex)) { // Load mini game if no reward and is in minigamesIndexes
                    // Show reward
                    playerMoveCount.gameObject.SetActive(true);
                    playerMoveCount.GetComponent<Text>().text = "Game Time!";

                    // Go to mini-game
                    if (currentIndex == 1) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CardScene");
                    } else if (currentIndex == 4) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CrossingScene");
                    } else if (currentIndex == 5) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "Whack-A-Mole");
                    } else if (currentIndex == 9) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "FruitScene");
                    } else if (currentIndex == 12) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "ParachuteScene");
                    } else if (currentIndex == 15) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "RhythmScene");
                    } else if (currentIndex == 19   ) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CorgiScene");
                    } else if (currentIndex == 21) {
                        sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "MosquitoScene");
                    }

                    StartCoroutine(disableDice());
                } else {
                    if (currentIndex == 3 || currentIndex == 13 || currentIndex == 23) {
                        shop.gameObject.SetActive(true); // go to store
                        isInStore = true;

                        // // hide test buttons
                        // GameObject.Find("TestGames").SetActive(false);
                    } else if (currentIndex == 22) {
                        // Show reward
                        playerMoveCount.gameObject.SetActive(true);
                        playerMoveCount.GetComponent<Text>().text = "Play a random game!";

                        // play one random mini game
                        int randomIndex = UnityEngine.Random.Range(0, 9);
                        switch (randomIndex)
                        {
                            case 0: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CardScene"); break;
                            case 1: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CrossingScene"); break;
                            case 2: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "Whack-A-Mole"); break;
                            case 3: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "FruitScene"); break;
                            case 4: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "ParachuteScene"); break;
                            case 5: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "RhythmScene"); break;
                            case 6: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "CorgiScene"); break;
                            case 7: sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "MosquitoScene"); break;

                        }
                    } else if (currentIndex == 11) {
                        playerMoveCount.GetComponent<Text>().text = "Skip";
                    } else if (currentIndex == 7) {
                        int newGameScore = 0;
                        playerMoveCount.GetComponent<Text>().text = "Lose all the money...";
                        PlayerPrefs.SetInt("totalGameScore", newGameScore);
                    } else if (currentIndex == 14) {
                        int newGameScore = currentGameScore * 2;
                        playerMoveCount.GetComponent<Text>().text = "Double your money!";
                        PlayerPrefs.SetInt("totalGameScore", newGameScore);
                    } else if (currentIndex == 6 || currentIndex == 16) {
                        int randomReward = UnityEngine.Random.Range(-100, 101);
                        int adjustedReward = randomReward * 5;

                        if (adjustedReward > 0) {
                            //Reward
                            playerMoveCount.GetComponent<Text>().text = "Surprise! You get $" + adjustedReward;
                        } else if (adjustedReward == 0) {
                            playerMoveCount.GetComponent<Text>().text = "Hmm... Nothing changes";
                        } else {
                            //Penalty
                            if (isInsuranceApplicable) {
                                playerMoveCount.GetComponent<Text>().text = "Your insurance saved you\nfrom penalty!";
                            } else if (currentGameScore + adjustedReward <= 0) {
                                playerMoveCount.GetComponent<Text>().text = "Oops! You lose all your money...";
                            } else {
                                playerMoveCount.GetComponent<Text>().text = "Oops! You lose $" + Mathf.Abs(adjustedReward);
                            }
                        }
                        // Adjust score
                        if (!isInsuranceApplicable) {
                            int newGameScore = Math.Max(currentGameScore + adjustedReward, 0);
                            PlayerPrefs.SetInt("totalGameScore", newGameScore);
                        } else {
                            PlayerStats.useOneItem(PlayerStats.insurance);
                        }
                    }
                    hasFinishedReward = true;
                }
            }
            
            return;
        }

        IEnumerator disableDice() {
            yield return new WaitForSeconds(1.5f);
            hasFinishedReward = true;
        }

        // Setup UI
        playerMoveCount.gameObject.SetActive(true);

        // Setup player movement
        if (rewardedSteps > 0) {
            playerMoveCount.GetComponent<Text>().text = "Move " + rewardedSteps + " more steps";
            player.GetComponent<PlayerMovement>().moveForward = true;
        } else if (rewardedSteps < 0) {
            if (currentIndex == 10) {
                playerMoveCount.GetComponent<Text>().text = "Move back to origin";
            } else {
                playerMoveCount.GetComponent<Text>().text = "Reverse " + Mathf.Abs(rewardedSteps) + " steps";
            }
            player.GetComponent<PlayerMovement>().moveForward = false;
        }

        player.GetComponent<PlayerMovement>().destinationWaypointIndex = Math.Max(player.GetComponent<PlayerMovement>().currentWaypointIndex + rewardedSteps, 0);
        player.GetComponent<PlayerMovement>().moveFinished = false;
        player.GetComponent<PlayerMovement>().moveAllowed = true;
    }

    public void GoToEndScene() {
        int totalgamescore = PlayerPrefs.GetInt("totalGameScore", 0);
        int endingMode = PlayerStats.getMode();
        // Debug.Log(endingMode);

        if (totalgamescore >= 10000 && endingMode != 3) {
            sceneAudio.PlayOneShot(changeSceneSound);
            sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "EndingDialogue3");
        } else if (totalgamescore >= 7500 && totalgamescore < 10000 && endingMode != 2) {
            sceneAudio.PlayOneShot(changeSceneSound);
            sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "EndingDialogue2");
        } else if (totalgamescore >= 5000 && totalgamescore < 7500 && endingMode != 1) {
            sceneAudio.PlayOneShot(changeSceneSound);
            sceneManager.GetComponent<SceneTransitions>().loadScene(sceneName: "EndingDialogue1");
        }
    }
}
