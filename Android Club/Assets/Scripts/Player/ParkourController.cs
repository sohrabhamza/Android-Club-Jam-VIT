using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

class ParkourController : MonoBehaviour
{
    //Serialized Variables
    [SerializeField] float VaultTime; //how long the vault takes

    [SerializeField] float ClimbTime; //how long the vault takes

    [SerializeField] float wallRunDetectionLength = .5f;
    [SerializeField] float wallRunSpeed = 7;
    [SerializeField] float wallRunExitSpeed = 3;
    [SerializeField] float maxCamTiltDegree = 4;
    [SerializeField] float maxWallRunTiltDegree = 10;
    [SerializeField] float camTiltSpeed = 3f;
    [SerializeField] LayerMask wallRunLayer;


    //Serialized references
    // [SerializeField] Animator cameraAnimator;
    [SerializeField] DetectObs detectVaultObject; //checks for vault object
    [SerializeField] DetectObs detectVaultObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to vault
    [SerializeField] DetectObs detectClimbObject; //checks for climb object
    [SerializeField] DetectObs detectClimbObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to climb
    [SerializeField] DetectObs detectWallL;
    [SerializeField] DetectObs detectWallR;
    [SerializeField] Transform vaultEndPoint;
    [SerializeField] Transform climbEndPoint;
    [SerializeField] Transform cameraHolder;
    // [SerializeField] TextMeshProUGUI debugText;

    //Private References
    CharacterController controller;
    PlayerMovementChCtrl playerController;

    //Private Variable
    bool IsParkour;
    float t_parkour;
    float chosenParkourMoveTime;
    bool CanClimb;
    bool CanVault;
    float currectCamRotation;
    Vector3 RecordedMoveToPosition; //the position of the vault end point in world space to move the player to
    Vector3 RecordedStartPosition; // position of player right before vault

    void Start()
    {
        playerController = GetComponent<PlayerMovementChCtrl>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!playerController.grounded)
        {
            if (!IsParkour && Input.GetKey(KeyCode.Space) && Input.GetAxisRaw("Vertical") > 0)
            {
                Climb();
                Vault();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && doWallRun)
        {
            WallRunExit();
            playerController.AddWallRunSpeedModifier();
        }

        if (IsParkour)
        {
            PerformParkour();
        }

        // cameraHolder.localEulerAngles += new Vector3(0, 0, 50) * Time.deltaTime;
        if (!doWallRun)
        {
            float tiltAmount = Mathf.Clamp(transform.InverseTransformDirection(controller.velocity).x, -1, 1);
            currectCamRotation = Mathf.Lerp(currectCamRotation, tiltAmount * maxCamTiltDegree, Time.deltaTime * camTiltSpeed);
            cameraHolder.rotation = Quaternion.Euler(cameraHolder.eulerAngles.x, cameraHolder.eulerAngles.y, currectCamRotation);
        }
        else
        {
            float tiltAmount = -Mathf.Clamp(transform.InverseTransformDirection(WallRunNormal()).x, -1, 1);
            currectCamRotation = Mathf.Lerp(currectCamRotation, tiltAmount * maxWallRunTiltDegree, Time.deltaTime * camTiltSpeed);
            cameraHolder.rotation = Quaternion.Euler(cameraHolder.eulerAngles.x, cameraHolder.eulerAngles.y, currectCamRotation);
        }
    }

    bool canwallrun;
    bool doWallRun;
    void FixedUpdate()
    {
        /*
        Input
        Wall obstruction 
        Velocity 
        Fully stopped wallrunning
        isParkour
        angle
        */

        if (!playerController.grounded && !IsParkour && canwallrun && (detectWallL.Obstruction || detectWallR.Obstruction))     //If player is not grounded, not parkouring, canwall run (has left a wall run fully and there is a wall to the side)
        {
            canwallrun = false;
            doWallRun = true;
        }

        if (doWallRun && ((!detectWallR.Obstruction && !detectWallL.Obstruction) || Input.GetAxisRaw("Vertical") < 1 /*|| Vector3.Dot(controller.velocity, wallRunDir()) < .75f*/))     //If wallruning, check if there is still an obstruction, input is given and travelling in correct direction
        {
            string test = "Left ob " + detectWallR.Obstruction + " Right Ob " + detectWallL.Obstruction + " " + Input.GetAxisRaw("Vertical") + " " + Vector3.Dot(controller.velocity, wallRunDir());
            Debug.Log(test);
            // debugText.text = test;
            WallRunExit();
        }

        if (doWallRun)  //If wallrunning wall run
        {
            WallRun();
        }

        if (!detectWallR.Obstruction && !detectWallL.Obstruction && !canwallrun)    //If wall run has been fully exited allow for wall run
        {
            canwallrun = true;
        }

        FindWallContactPoint();     //Look for wall contact for normal
    }

    void WallRun()
    {
        playerController.isEnabled = false;     //Disable player movement 
        // gunHider.skipHideTimer = true;
        playerController.ResetMovement();   //Reset any previous movement

        playerController.WallRun(wallRunDir(), wallRunSpeed);   //Ask player to wall run in given direction with given speed

        // Debug.Log(Vector3.Angle(WallRunNormal(), transform.forward));
        if (playerController.grounded || Vector3.Angle(WallRunNormal(), transform.forward) > 160 /*|| Vector3.Angle(WallRunNormal(), transform.forward) < 40*/)     //If player becomes grounded, or angle exeeds then eject
        {
            string test = "Grounded " + playerController.grounded + " " + Vector3.Angle(WallRunNormal(), transform.forward);
            Debug.Log(test);
            // debugText.text = test;
            WallRunExit();
        }
    }

    Vector3 oldWallRunDir;
    void WallRunExit()
    {
        doWallRun = false;

        playerController.isEnabled = true;
        // gunHider.skipHideTimer = false;

        playerController.ResetWallRun(WallRunNormal() + oldWallRunDir, wallRunExitSpeed);

        oldWallRunDir = Vector3.zero;
        wallContactPoint = Vector3.zero;
    }

    void Climb()
    {
        //climb
        if (detectClimbObject.Obstruction && !detectClimbObstruction.Obstruction && !CanClimb)
        {
            CanClimb = true;
            playerController.ResetMovement();
        }

        if (CanClimb)
        {
            CanClimb = false; // so this is only called once
            playerController.isEnabled = false; //ensure physics do not interrupt the climb
            RecordedMoveToPosition = climbEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = ClimbTime;

            // cameraAnimator.CrossFade("Climb", 0.1f);
        }
    }

    void Vault()
    {
        //vault
        if (detectVaultObject.Obstruction && !detectVaultObstruction.Obstruction && !CanVault)
        // if detects a vault object and there is no wall in front then player can pressing space or in air and pressing forward
        {
            CanVault = true;
            playerController.ResetMovement();
        }

        if (CanVault)
        {
            CanVault = false; // so this is only called once
            playerController.isEnabled = false; //ensure physics do not interrupt the vault
            RecordedMoveToPosition = vaultEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = VaultTime;

            // cameraAnimator.CrossFade("Vault", 0.1f);
        }

    }

    void PerformParkour()
    {
        //Parkour movement
        if (IsParkour && t_parkour < 1f)
        {
            t_parkour += Time.deltaTime / chosenParkourMoveTime;
            transform.position = Vector3.Lerp(RecordedStartPosition, RecordedMoveToPosition, t_parkour);
            // gunHider.skipHideTimer = true;

            if (t_parkour >= 1f)
            {
                IsParkour = false;
                t_parkour = 0f;
                playerController.isEnabled = true;
                // gunHider.skipHideTimer = false;
            }
        }
    }


    Vector3 wallContactPoint;
    void FindWallContactPoint()     //Finds wall contact point
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0.25f), transform.right, out RaycastHit wallHitRight, wallRunDetectionLength, wallRunLayer))
        {
            Debug.DrawRay(transform.position + new Vector3(0, 1, 0.25f), transform.right * wallRunDetectionLength, Color.blue);
            wallContactPoint = wallHitRight.point;
        }
        else if (Physics.Raycast(transform.position + new Vector3(0, 1, 0.25f), -transform.right, out RaycastHit wallHitLeft, wallRunDetectionLength, wallRunLayer))
        {
            Debug.DrawRay(transform.position + new Vector3(0, 1, 0.25f), -transform.right * wallRunDetectionLength, Color.green);
            wallContactPoint = wallHitLeft.point;
        }
    }

    Vector3 WallRunNormal()     //Finds wall normal
    {
        Physics.Raycast(transform.position, (wallContactPoint - transform.position).normalized, out RaycastHit wallHit, 100, wallRunLayer);
        wallContactPoint = wallHit.point;
        return wallHit.normal;
    }

    Vector3 wallRunDir()        //Gets wall run direction for force to be applied
    {
        Vector3 planeUp = Vector3.Cross(WallRunNormal(), transform.forward);
        Vector3 dirOne = Vector3.Cross(WallRunNormal(), planeUp);
        Vector3 dirTwo = -dirOne;

        if (Vector3.Dot(dirOne, transform.forward) > Vector3.Dot(dirTwo, transform.forward))
        {
            dirOne.Normalize();
            Debug.DrawRay(transform.position, dirOne, Color.red);
            oldWallRunDir = dirOne;
            return dirOne;
        }
        else
        {
            dirTwo.Normalize();
            Debug.DrawRay(transform.position, dirTwo, Color.red);
            oldWallRunDir = dirTwo;
            return dirTwo;
        }
    }
}
