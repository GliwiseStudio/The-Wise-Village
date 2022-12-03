using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class police : MonoBehaviour
{
    [SerializeField] private Animation _animator;
    private PoliceAnimationsHandler _animationsHandler;
    private Locator _locator;


    [SerializeField] private GameObject thief;

    private enum States { Patrolling, FollowingThief, KnockingThief};
    private States actualState;


    //position of the flags in the world (interessZones)
    private Vector3[] PatrollingPoints = {  new Vector3 (-63.5f, 0f, -11.3f),
                                            new Vector3 (-93.4f, 0f, -50f),
                                            new Vector3 (18.9f, 0f, -58.2f),
                                            new Vector3 (12.7f, 0f, -14.8f),
                                            new Vector3 (-7.5f, 0f, 5.9f),
                                            new Vector3 (-18.3f, 0f, -103.4f)};
    private int patrolPoint;


    private int visionRange;
    private int walkVelocity;
    private int runVelocity;


    private void Awake()
    {
        //_animationsHandler = new PoliceAnimationsHandler(_animator);      ????
        _locator = FindObjectOfType<Locator>();

        actualState = States.Patrolling;
    }

    // Update is called once per frame
    void Update()
    {
        //Its a FSM 
        FSMPolice();
    }

    void FSMPolice()
    {
        switch (actualState)
        {
            case (States.Patrolling):

                //perception there is a thief
                if (watchedThiefPerception())           //posible solution
                {
                    actualState = States.FollowingThief;

                    FollowingTheThiefWorld();
                }

                //perception i am patrolling
                else if(policeOnPatrollPointPerception(patrolPoint))
                {
                    //nuevo punto a patrullar
                    patrolPoint = Random.Range(0, 5);

                    LookToPoint(PatrollingPoints[patrolPoint]);
                }

                else
                { 
                    MoveToPatrollPoint();
                }

            break;
            case (States.FollowingThief):

                //perception i am following the thief
                if (PoliceCatchedTheThiefPerception())
                {
                    actualState = States.KnockingThief;

                    KnokingThief();

                    KnockingTheThiefWorld();
                }

                //perception i have lost the thief
                else if (PoliceLostThiefPerception())
                {
                    actualState = States.Patrolling;

                    PoliceHasLostTheThiefWorld();
                }


                else
                {
                    LookToPoint(thief.transform.position);

                    RunToThief();
                }


                break;
            case (States.KnockingThief):
                
                //perception i have knocked the thief
                if (IHaveKnockedTheThiefPerception())
                {
                    actualState = States.Patrolling;
                }

                break;

            default:
                Debug.Log("Error Policia, estado no posible");
                break;
        }


    }

    //---------------------------------------------------
    //--------------------PERCEPTIONS--------------------
    //---------------------------------------------------

    private bool policeOnPatrollPointPerception(int point)
    {
        //Check if the police is on one of the patroll points

        if (transform.position == PatrollingPoints[point])

        { return true; }

        else { return false; }
    }

    private bool watchedThiefPerception()
    {
        //check thiefs position and if it is in range and stealing
        return false;
    }

    private bool PoliceCatchedTheThiefPerception()
    {
        //if police is at less than 2 meters away of the thief
        if (transform.position.x - thief.transform.position.x < 2 || transform.position.z - thief.transform.position.z < 2)
        {
            return true;
        }
        else return false;
    }

    private bool PoliceLostThiefPerception()
    {
        //if thief is out of range
        //(it is posible to send him to a house an in there send him to the ground to get him out of range)
        return false;
    }

    private bool IHaveKnockedTheThiefPerception()
    {
        //if animacion knock thief is over return true
        return false;
    }

    //-----------------------------------------------------------
    //--------------------WORLD ANNOUNCEMENTS--------------------
    //-----------------------------------------------------------

    private void FollowingTheThiefWorld()
    {
        //Claim the world I am following the Thief!
    }

    private void PoliceHasLostTheThiefWorld()
    {
        //Claim the world I have lost the thief! (dang it)
    }

    private void KnockingTheThiefWorld()
    {
        //Claim the world I am knocking the thief!
    }

    //-----------------------------------------------
    //--------------------ACTIONS--------------------
    //-----------------------------------------------

    private void LookToPoint(Vector3 point)
    {
        //rotation to point
    }

    private void MoveToPatrollPoint()
    {
        //animation walking
    }

    private void RunToThief()
    {
        //animation running
    }

    private void KnokingThief()
    {
        //animation knock the thief (brutal attack, kick or punch)
    }

}
