using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ObjectiveManager : MonoBehaviour {

    private AudioSource audioSource;
    private int waveCount = 0;
    private float timeLeft = 5.0f;
    private bool startCountdown = false;
    private float flashCooldown = 0.0f;
    private bool inWave = false;

    public AudioClip greetingAudio;
    public float greetingAudioDelay = 2.5f;
    public float hideTutorialDelay = 30.0f;

    public AudioClip startWaveAudio;
    public AudioClip endWaveAudio;
    public AudioClip narratorEndWave;
    public AudioClip narratorIncomingPlanes;
    public AudioClip narratorGreatJob;

    public GameObject AirTrafficPlayer;
    public GameObject AirTrafficBase;
    
    public GameObject HUDTutorial;
    public GameObject HUDWaveIncoming;
    public GameObject HUDWaveCount;
    public GameObject HUDWaveTimer;
    public GameObject HUDWaveKilled;
    public Text waveText;
    public Text timerText;
    public Text killedText;
    public Text incomingText;

    public bool gameOver = false;


	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(playGreeting(greetingAudioDelay));
        StartCoroutine(hideTutorial(hideTutorialDelay));

        HUDTutorial.SetActive(true);
        HUDWaveIncoming.SetActive(false);
        HUDWaveCount.SetActive(true);
        HUDWaveTimer.SetActive(false);
        HUDWaveKilled.SetActive(false);

        AirTrafficPlayer.SetActive(false);
        //AirTrafficBase.SetActive(false);

        inWave = false;
        updateWave(0);
    }

    public void GameOver() {
        gameOver = true;
        HUDTutorial.SetActive(false);
        HUDWaveCount.SetActive(true);
        HUDWaveTimer.SetActive(false);
        HUDWaveKilled.SetActive(false);
        AirTrafficPlayer.SetActive(false);

        HUDWaveIncoming.SetActive(true);
        incomingText.text = "GAME OVER";
        incomingText.color = new Color(255, 0, 0);
    }

    void updateWave(int wave) {
        waveCount = wave;
        waveText.text = "Wave " + waveCount;
    }

    void startFirstWave() {
        HUDWaveTimer.SetActive(true);
        startCountdown = true;
        inWave = false;
    }

    void commenceWave() {
        HUDWaveIncoming.SetActive(false);
        HUDWaveTimer.SetActive(false);

        HUDWaveKilled.SetActive(true);
        inWave = true;

        //audioSource.clip = startWaveAudio;
        //audioSource.Play();
        StartCoroutine(playStartWaveAudio());


        //AirTrafficPlayer.SetActive(true);
        //AirTrafficBase.SetActive(true);

        if (waveCount == 0) {
            updateWave(waveCount + 1);
            AirTrafficPlayer.GetComponent<AirTrafficControl>().setPlaneData(0, 5, 1);
        }
        else if (waveCount == 1) {
            AirTrafficPlayer.GetComponent<AirTrafficControl>().setPlaneData(0, 10, 1);
        }
        else {
            AirTrafficPlayer.GetComponent<AirTrafficControl>().setPlaneData(0, 5, 1);
        }
        AirTrafficPlayer.SetActive(true);
        //AirTrafficBase.SetActive(true);
    }

    void completeWave() {
        inWave = false;

        HUDWaveKilled.SetActive(false);
        updateWave(waveCount + 1);
        timeLeft = 15.0f;
        HUDWaveTimer.SetActive(true);
        startCountdown = true;

        StartCoroutine(playEndWaveAudio()); // play sound

    }

    IEnumerator playEndWaveAudio() {
        audioSource.clip = endWaveAudio;
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.clip = narratorEndWave;
        audioSource.Play();
        if(Random.Range(0, 1) == 0) {
            yield return new WaitForSeconds(audioSource.clip.length);
            audioSource.clip = narratorGreatJob;
            audioSource.Play();
        }
    }

    IEnumerator playStartWaveAudio() {
        audioSource.clip = startWaveAudio;
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.clip = narratorIncomingPlanes;
        audioSource.Play();
    }

    void Update() {

        if (gameOver) return;
        if (inWave) {
            int numberJets = GameObject.FindGameObjectsWithTag("fighterJet").Length;
            killedText.text = numberJets + " enemies";
            if (numberJets == 0) {
                completeWave();
                // Do something
            }
        }

        if (startCountdown) {
            if(!HUDWaveIncoming.activeInHierarchy) {
                if(Time.time > flashCooldown + 0.5f) {
                    flashCooldown = Time.time;
                    HUDWaveIncoming.SetActive(true);
                }
            }
            else {
                if (Time.time > flashCooldown + 0.5f) {
                    flashCooldown = Time.time;
                    HUDWaveIncoming.SetActive(false);
                }
            }
            timeLeft -= Time.deltaTime;
            System.TimeSpan t = System.TimeSpan.FromSeconds(timeLeft);
            string timerFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            timerText.text = timerFormatted;
            if(timeLeft <= 0) {
                // Activate something
                startCountdown = false;
                commenceWave();
            }
        }
    }

    IEnumerator playGreeting(float delay) {
        yield return new WaitForSeconds(delay);
        audioSource.clip = greetingAudio;
        audioSource.Play();
        startFirstWave();
    }

    IEnumerator hideTutorial(float delay) {
        yield return new WaitForSeconds(delay);
        HUDTutorial.SetActive(false);
    }
	
}
