using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialoguePanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text textUI;
    CanvasGroup cg;

    [Header("Typewriter")]
    [SerializeField] float charInterval = 0.03f;

    string[] lines;
    int lineIndex;

    bool isTyping;
    bool isPlaying;

    Action onComplete;
    DialogueAsset playingAsset;

    Coroutine typingRoutine;

    public bool IsPlaying => isPlaying;

        void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Play(DialogueAsset asset, Action onComplete = null)
    {
        playingAsset = asset;
        PlayInternal(asset.lines, onComplete);
    }

    public void Play(string[] lines, Action onComplete = null)
    {
        PlayInternal(lines, onComplete);
    }

    void PlayInternal(string[] lines, Action onComplete)
    {
        this.lines = lines;
        this.onComplete = onComplete;

        lineIndex = 0;
        isPlaying = true;

        Show();
        ShowLine(0);
    }

    public void Stop()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isTyping = false;
        isPlaying = false;
        Hide();

        lines = null;
        onComplete = null;
        playingAsset = null;
    }

    // 외부(튜토리얼 매니저)에서 "확인" 입력을 넣어줄 용도
    public void Confirm()
    {
        if (!isPlaying)
            return;

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        lineIndex++;

        if (lines == null || lineIndex >= lines.Length)
        {
            Finish();
            return;
        }

        ShowLine(lineIndex);
    }

    void ShowLine(int index)
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        typingRoutine = StartCoroutine(TypeRoutine(lines[index]));
    }

    IEnumerator TypeRoutine(string message)
    {
        isTyping = true;

        textUI.text = message;
        textUI.maxVisibleCharacters = 0;

        int total = message.Length;

        for (int i = 0; i <= total; i++)
        {
            textUI.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(charInterval);
        }

        isTyping = false;
        typingRoutine = null;
    }

    void SkipTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        // 현재 줄을 즉시 전부 보이게
        textUI.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    void Finish()
    {
        isPlaying = false;

        Hide();

        // 에셋에 박아둔 인스펙터 이벤트
        if (playingAsset != null)
            playingAsset.onFinished?.Invoke();

        // 호출자가 넘긴 콜백(튜토리얼 단계 완료 같은 거)
        Action cb = onComplete;

        lines = null;
        onComplete = null;
        playingAsset = null;

        cb?.Invoke();
    }
    
    void Show()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
