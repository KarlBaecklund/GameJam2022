using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class MovementScript : MonoBehaviour
{
    private CharacterInformation charInfo;
    private Rigidbody rb;
    [SerializeField] private Vector3 _gravity = new Vector3(0, -5, 0);
    [SerializeField] private float raycastOffset = 0.5f;
    private const int _MULTIPLIER = 200;

    private Vector3 _moveVec;
    private bool _isJumping = false;
    private bool _canClimb = false;
    private bool _isClimbing = false;
    private Coroutine c_jumpCooldown;

    [Header("PowerUp Things")]
    public bool PowerUp1;
    public bool PowerUp2;
    [SerializeField] private float _doubleJumpTime = 10;
    public float tagBlockDuration = 10;
    [SerializeField] private float _cleanAndGraffitiBuffTime = 10;

    WallDetector wallDetector = new WallDetector();

    public Animator _anim = null;
    private void Start()
    {
        charInfo = GetComponent<CharacterInformation>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(!_anim)
        {
            return;
        }
        if (charInfo.es.isActiveAndEnabled) return;
        Vector3 newVel = Vector3.zero;
        if (_isClimbing) //climbing
        {
            if (_moveVec.magnitude > 0.1f)
            {
                _anim.SetBool("Climbing", true);
                newVel = new Vector3(0, charInfo._climbSpeed);
            }
        }
        else if (CheckIfInAir()) //falling
        {
            newVel = new Vector3(_moveVec.x, _gravity.y, _moveVec.z);
        }
        else //moving and or jumping
        {
            newVel = new Vector3(_moveVec.x, _isJumping ? charInfo._jumpSpeed : 0, _moveVec.z);
        }
        _anim.SetFloat("Speed", _moveVec.magnitude);
        rb.velocity = newVel * Time.fixedDeltaTime * _MULTIPLIER;
    }
    private bool CheckIfInAir()
    {
        if (_isClimbing)
        {
            return false;
        }
        Vector3[] positions =
        {
            transform.position,
            new Vector3(transform.position.x - raycastOffset, transform.position.y, transform.position.z),
            new Vector3(transform.position.x + raycastOffset, transform.position.y, transform.position.z),
            new Vector3(transform.position.x, transform.position.y, transform.position.z - raycastOffset),
            new Vector3(transform.position.x, transform.position.y, transform.position.z + raycastOffset),
            new Vector3(transform.position.x - raycastOffset, transform.position.y, transform.position.z - raycastOffset),
            new Vector3(transform.position.x + raycastOffset, transform.position.y, transform.position.z + raycastOffset),
            new Vector3(transform.position.x + raycastOffset, transform.position.y, transform.position.z - raycastOffset),
            new Vector3(transform.position.x - raycastOffset, transform.position.y, transform.position.z + raycastOffset)
        };

        foreach (Vector3 pos in positions)
        {
            
        }
        int rayInAir = 0;
        for (int i = 0; i < 9; i++)
        {
            Ray ray = new Ray(positions[i], Vector3.down);
            bool didRayHit = Physics.Raycast(ray, out RaycastHit hit);
            if (didRayHit && Mathf.Abs(hit.point.y - transform.position.y) > (charInfo._characterHeight + 0.2f) / 2)
            {
                rayInAir++;
            }
            if (rayInAir == 9)
            {
                return true;
            }
        }
        return false;
    }
    public void OnMoving(InputValue c) //add set forward to this c 
    {
        Vector2 normDirection = c.Get<Vector2>().normalized;
        Vector3 direction = new Vector3(normDirection.x, 0, normDirection.y);
        Vector3 dirRelativeCamera = Camera.main.transform.TransformDirection(direction);
        if (new Vector3(dirRelativeCamera.x, transform.forward.y, dirRelativeCamera.z) != transform.forward && new Vector3(dirRelativeCamera.x, 0, dirRelativeCamera.z) != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(dirRelativeCamera.x, transform.forward.y, dirRelativeCamera.z), Vector3.up);
        }
        _moveVec = dirRelativeCamera * charInfo._movementSpeed;
    }

    public void OnJumpClimb()
    {
        if (CheckIfInAir())
        {
            return;
        }
        //if close to climbing area
        if (_canClimb && !_isClimbing)
        {
            _isClimbing = true;
            return;
        }
        else if (c_jumpCooldown == null)
        {
            _isJumping = true;
            _anim.SetTrigger("Jump");
            c_jumpCooldown = StartCoroutine(JumpCooldown());
        }
        else
        {
            _isClimbing = false;
        }
    }
    private IEnumerator JumpCooldown() //if doublejump we gotta 
    {
        _gravity = Vector3.zero;
        yield return new WaitForSeconds(charInfo._jumpDuration);
        _gravity = new Vector3(0, -5, 0);
        while (CheckIfInAir())
        {
            yield return null;
        }
        _isJumping = false;
        yield return new WaitForSeconds(charInfo._jumpCooldown);
        c_jumpCooldown = null;
    }
    private IEnumerator DoubleJumpActive()
    {
        charInfo._jumpSpeed *= 2.5f;
        yield return new WaitForSeconds(_doubleJumpTime);
        charInfo._jumpSpeed /= 2.5f;
    }
    public void OnGraffitiClean(InputValue c) //might not be able to do callback context, just check information in controller scheme?
    {
        if (charInfo._character == CharacterENUM.MORT)
        {
            //test do clean
            _anim.SetTrigger("CleaningAndGraffiting");
            wallDetector.interactingWithWall(transform, ref c, charInfo);
        }
        else
        {
            //test do graffiti
            wallDetector.interactingWithWall(transform, ref c, charInfo);
        }
    }
    private IEnumerator CleanOrGraffitiMulti()
    {
        charInfo._cleanOrGraffitiMultiplier *= 2;
        yield return new WaitForSeconds(_cleanAndGraffitiBuffTime);
        charInfo._cleanOrGraffitiMultiplier /= 2;
    }
    //fast-tag amd speedy clean
    public void OnPowerUp1()
    {
        if (PowerUp1)
        {
            if (charInfo._character == CharacterENUM.MORT)
            {
                StartCoroutine(CleanOrGraffitiMulti());
            }
            else
            {
                StartCoroutine(CleanOrGraffitiMulti());
            }
        }
    }
    //doublejump & no tag!
    public void OnPowerUp2(InputValue c)
    {
        if (PowerUp2)
        {
            if (charInfo._character == CharacterENUM.MORT)
            {
                wallDetector.interactingWithWall(transform, ref c, charInfo, true);
            }
            else
            {
                Debug.Log("PowerUP used");
                UIManager.UI.SetPowerUpImage(UIManager.UI.tildaImage2, false);
                PowerUp2 = false;
                StartCoroutine(DoubleJumpActive());
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Climbable"))
        {
            _canClimb = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Climbable"))
        {
            _canClimb = false;
            _isClimbing = false;
            _anim.SetBool("Climbing", false);
        }
    }
}
