using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimatorBridge : MonoBehaviour
{
    // Assigned in Editor (via Inspector or Lab5AnimationSetup Step 3)
    [SerializeField] public RuntimeAnimatorController controllerAsset;
    [SerializeField] public Avatar avatarAsset;

    private PlayerController _pc;
    private Rigidbody        _rb;
    private Animator         _anim;
    private bool             _dead;
    private bool             _celebrating;

    void Start()
    {
        _pc  = GetComponent<PlayerController>();
        _rb  = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();

        // Ensure controller and avatar are always assigned at runtime
        if (_anim != null)
        {
            if (controllerAsset != null)
                _anim.runtimeAnimatorController = controllerAsset;
            if (avatarAsset != null)
                _anim.avatar = avatarAsset;
            // UnscaledTime so death animation plays even when timeScale = 0
            _anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        GameStore.OnGameOver += OnGameOver;
    }

    void OnDestroy()
    {
        GameStore.OnGameOver -= OnGameOver;
    }

    void Update()
    {
        if (_anim == null || _dead) return;

        float speed = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).magnitude;
        if (_pc.IsBoosting) speed = 10f;
        _anim.SetFloat("Speed", speed);
        _anim.SetBool("IsGrounded", _pc.IsGrounded);
    }

    public void TriggerHit()
    {
        if (_anim == null || _dead) return;
        _anim.SetTrigger("Hit");
    }

    public void TriggerCelebrate()
    {
        if (_celebrating || _dead) return;
        _celebrating = true;
        if (_anim != null) _anim.SetBool("IsCelebrating", true);
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic    = true;
    }

    private void OnGameOver()
    {
        if (_dead) return;
        _dead = true;
        StartCoroutine(PlayDeathSequence());
    }

    private IEnumerator PlayDeathSequence()
    {
        _pc.enabled        = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic    = true;

        if (_anim != null)
            _anim.SetBool("IsDead", true);

        yield return new WaitForSecondsRealtime(3f);
        if (_anim != null) _anim.enabled = false;
    }
}
