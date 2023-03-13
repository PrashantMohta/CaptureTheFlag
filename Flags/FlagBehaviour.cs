using Hkmp.Game;
using Satchel;
using System;
using System.Collections;
using UnityEngine;

namespace CaptureTheFlag.Flags
{
    internal class FlagBehaviour : MonoBehaviour
    {
        CircleCollider2D trigger;
        public bool IsCapturing = false;
        public bool IsCaptured = false;
        public DateTime StartCapture;
        public int TeamId;

        public DateTime StartDeposit { get; private set; }

        public bool IsDepositing = false;
        public bool IsDeposited = false;

        public event EventHandler<FlagEventArgs> OnFlagCaptured;
        public event EventHandler<FlagEventArgs> OnFlagDeposited;
        public SpriteRenderer sr = null;

        Sprite TeamSprite;
        Sprite TakenSprite;
        public void Awake()
        {
            StartCapture = DateTime.MinValue;
            IsCapturing = false;
            IsCaptured = false;
            if (trigger == null)
            {
                gameObject.layer = 13;
                trigger = gameObject.GetAddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = Constants.DefaultFlagRange; // 3 unit radius for the thing
            }
        }


        private Coroutine _flashRoutine;

        IEnumerator Flash(Color FlashColor,float time)
        {
            float flashAmount = 1.0f;
            Material material = sr.material;
            while (flashAmount > 0)
            {
                material.SetFloat("_FlashAmount", flashAmount);
                material.SetColor("_Color", FlashColor);
                material.SetFloat("_SelfIllum", 0.4f);
                flashAmount -= 0.01f;
                if (flashAmount <= 0)
                {
                    material.SetFloat("_FlashAmount", 0f);
                    material.SetColor("_Color", new Color(1, 1, 1, 1));
                    material.SetFloat("_SelfIllum", 0f);
                }
                yield return new WaitForSeconds(0.01f * time);
            }

            yield return null;
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }
        public void StartFlashColor(Color c)
        {
            if (sr.material.shader != Core.spriteFlash)
            {
                CaptureTheFlag.Instance.Log("Cannot FlashSprite because the spriteFlash shader is not being used");
                return;
            }
            if (_flashRoutine == null)
            {
                _flashRoutine = StartCoroutine(Flash(c, 0.1f));
            }
            
        }

        public Sprite getTeamSprite()
        {
            if (TeamSprite == null)
            {
                TeamSprite = AssemblyUtils.GetSpriteFromResources($"{((Team)TeamId).ToString().ToLower()}.png");
            }
            return TeamSprite;
        }
        public Sprite getTakenSprite()
        {
            if (TakenSprite == null)
            {
                TakenSprite = AssemblyUtils.GetSpriteFromResources($"taken.png");
            }
            return TakenSprite;
        }
        public void Update()
        {
            if(sr == null && TeamId > 0)
            {
                sr = gameObject.GetAddComponent<SpriteRenderer>();
                sr.enabled = true;
            } else { 
                if (!IsCaptured)
                {
                    sr.sprite = getTeamSprite();
                    sr.material.shader = Core.spriteFlash;
                }
                else
                {
                    sr.sprite = getTakenSprite();
                    sr.material.shader = Core.spriteFlash;
                }
            }
        }
        public void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject == HeroController.instance.gameObject)
            {

                if (TeamId != (int)CaptureTheFlag.pipe.ClientApi.ClientManager.Team && !IsCaptured && !ClientGameManager.HasFlag) {  // pick other's flags
                    if (IsCapturing)
                    {
                        StartFlashColor(new Color(0.7f, 0.5f, 0.5f));
                    }
                    if (StartCapture == DateTime.MinValue )
                    {
                        StartCapture = DateTime.Now;
                        IsCapturing = true;
                    }
                    if ((DateTime.Now - StartCapture).TotalSeconds >= Constants.DefaultFlagPick)
                    {
                        IsCapturing = false;
                        IsCaptured = true;
                        StartCapture = DateTime.MinValue;
                        OnFlagCaptured?.Invoke(this, new FlagEventArgs { TeamId = TeamId,IsCaptured = IsCaptured });
                    }
                }
                if (TeamId == (int)CaptureTheFlag.pipe.ClientApi.ClientManager.Team && !IsDeposited && ClientGameManager.HasFlag) // deposit other's flags
                {
                    if (IsDepositing)
                    {
                        StartFlashColor(new Color(0.5f, 0.5f, 0.5f));
                    }
                    if (StartDeposit == DateTime.MinValue)
                    {
                        StartDeposit = DateTime.Now;
                        IsDepositing = true;
                    }
                    if ((DateTime.Now - StartDeposit).TotalSeconds >= Constants.DefaultFlagDeposit)
                    {
                        IsDepositing = false;
                        IsDeposited = true;
                        StartDeposit = DateTime.MinValue;
                        OnFlagDeposited?.Invoke(this, new FlagEventArgs { TeamId = TeamId,IsDeposited = IsDeposited });
                    }
                }
            }

        }
        public void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject == HeroController.instance.gameObject)
            {
                StartCapture = DateTime.MinValue;
                IsCapturing = false;
                //IsCaptured = false; <= will be set via a server event
                StartDeposit = DateTime.MinValue;
                IsDepositing = false;
                IsDeposited = false;
            }
        }
    }
}
