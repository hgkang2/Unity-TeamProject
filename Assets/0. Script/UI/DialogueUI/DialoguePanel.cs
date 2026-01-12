using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DialoguePanel : UIPanelBase
{
    [Header("UI")]
    [SerializeField] TMP_Text textUI;

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

    protected override void Init()
    {
        Close();
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

        Open();
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
        Close();

        lines = null;
        onComplete = null;
        playingAsset = null;
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

        // 에셋에 박아둔 인스펙터 이벤트
        if (playingAsset != null)
            playingAsset.onFinished?.Invoke();

        // 호출자가 넘긴 콜백(튜토리얼 단계 완료 같은 거)
        Action cb = onComplete;

        lines = null;
        onComplete = null;
        playingAsset = null;

        Close();
        cb?.Invoke();
    }
    public override void OnUIInputConfirm()
    {
        // 대화가 진행 중이 아니면 리턴
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
    public override void OnUIInputCancel()
    {
        OnUIInputConfirm();
    }
}
