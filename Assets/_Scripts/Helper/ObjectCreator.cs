﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit;




/// <summary>
///  Helper class to use Functions from MonoBehaviour in ObjectManager
/// </summary>
public class ObjectCreator : ScriptableObject
{
    public string PrefabFolderName { get => prefabFolderName; set => prefabFolderName = value; }
    public string BoundingBoxFolderName { get => boundingBoxFolderName; set => boundingBoxFolderName = value; }
    public List<GameObject> InstantiatedObjects { get => instantiatedObjects; set => instantiatedObjects = value; }
    
    private string boundingBoxFolderName;
    private string soundFolderName; 
    private string prefabFolderName;
    private List<GameObject> instantiatedObjects;

    AudioClip rotateStart;
    AudioClip rotateStop; 

    #region public methods
    public ObjectCreator()
    {
        instantiatedObjects = new List<GameObject>();

        prefabFolderName = "Objects";
        boundingBoxFolderName = "BoundingBox";
        soundFolderName = "Sound";

        
    }

    public void OnEnable()
    {
        // Audio Sounds
        var rotfileName = "/MRTK_Rotate_Start";
        rotateStart = Resources.Load<AudioClip>(soundFolderName + rotfileName);
        if (rotateStart == null)
            throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file {0} found", rotfileName);

        var fileName = "/MRTK_Rotate_Stop";
        rotateStop = Resources.Load<AudioClip>(soundFolderName + fileName);
        if (rotateStop == null)
            throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file {0} found", fileName);
    }


    public void SpawnObject(GameObject obj, GameObject parent, Vector3 position, Quaternion rotation, ConfigType config)
    {

        ApplyRelevantComponents(obj);
        ApplyConfiguration(obj, config);

        var generatedObject = Instantiate(obj, position, rotation);


        // Add Sounds to Movement
        generatedObject.GetComponent<BoundsControl>().RotateStarted.RemoveAllListeners(); 
        generatedObject.GetComponent<BoundsControl>().RotateStarted.AddListener(() => {
            generatedObject.GetComponent<BoundsControl>().GetComponent<AudioSource>().PlayOneShot(rotateStart);
            Debug.Log("Manipulated.............................");
        });

        generatedObject.GetComponent<BoundsControl>().RotateStopped.RemoveAllListeners();
        generatedObject.GetComponent<BoundsControl>().RotateStopped.AddListener(() => {
            generatedObject.GetComponent<BoundsControl>().GetComponent<AudioSource>().PlayOneShot(rotateStop);
            Debug.Log("Manipulated.............................");
        });

        generatedObject.GetComponent<ObjectManipulator>().OnManipulationStarted.RemoveAllListeners();
        generatedObject.GetComponent<ObjectManipulator>().OnManipulationStarted.RemoveAllListeners();

        generatedObject.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener(HandleOnManipulationStarted); 
        generatedObject.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(HandleOnManipulationStopped);


        generatedObject.SetActive(true);



        generatedObject.name = generatedObject.name.Replace("(Clone)", ""); 
        generatedObject.transform.parent = parent.transform;
        generatedObject.transform.localPosition = position;
        generatedObject.transform.localRotation = rotation;

        
        instantiatedObjects.Add(generatedObject);
    }

    public void SpawnObjects(GameObject[] gameObjects, GameObject parent, Vector3[] positions, Quaternion[] rotations, ConfigType config)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            SpawnObject(gameObjects[i], parent, positions[i], rotations[i], config); 
        }
    }
    public void SpawnObjects(GameObject[] gameObjects, GameObject parent, Vector3 position, Quaternion rotation, ConfigType config)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            SpawnObject(gameObjects[i], parent, position, rotation, config);
        }
    }

    public GameObject CreateInteractionObject(ObjectData currentData)
    {
        // set first object in list to test Object
        GameObject loadedObj = Resources.Load<GameObject>(prefabFolderName + "/" + currentData.gameObjects[0].Objectname.ToString());

        if (loadedObj == null)
        {
            throw new FileNotFoundException("... ObjectManager::CreateInteractionObject no file found");
        }

        return loadedObj;
    }

    public GameObject[] CreateInteractionObjects(ObjectData currentData)
    {
        int length = currentData.gameObjects.Count;
        GameObject[] objs = new GameObject[length];

        for (int i = 0; i < length; i++)
        {
            var loadedObj = Resources.Load<GameObject>(prefabFolderName + "/" + currentData.gameObjects[i].Objectname.ToString());
            if (loadedObj == null)
            {
                throw new FileNotFoundException("... ObjectManager::CreateInteractionObjects Object " + currentData.gameObjects[i].Objectname.ToString() + " not found");
            }
            else
            {
                objs[i] = loadedObj;
            }
        }

        return objs;
    }

    public void RemoveAllObjects()
    {
        foreach (GameObject obj in instantiatedObjects)
        {
            Destroy(obj);
        }

        instantiatedObjects.Clear();
    }

    public void Reset()
    {
        if (instantiatedObjects != null)
            RemoveAllObjects(); 

    }

    #endregion public methods

    #region private methods

    private void ApplyConfiguration(GameObject obj, ConfigType config)
    {

        if (config == ConfigType.MovementDisabled)
        {
            try
            {
                if (obj.TryGetComponent(out BoundsControl bC))
                    bC.enabled = false;
                if (obj.TryGetComponent(out ObjectManipulator oM))
                    oM.enabled = false;
                if (obj.TryGetComponent(out NearInteractionGrabbable iG))
                    iG.enabled = false;
                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.useGravity = false;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                    

            }
            catch (InvalidCastException e)
            {
                throw new System.MemberAccessException("ObjectCreator:: ApplyConfiguration, not all Components found.", e);
            }
        }
        else if (config == ConfigType.MovementEnabled)
        {
            try
            {

                var comp = (BoundsControl)obj.GetComponent(typeof(BoundsControl));
                comp.enabled = true;

                var oM = (ObjectManipulator)obj.GetComponent(typeof(ObjectManipulator));
                oM.enabled = true;

                var iG = (NearInteractionGrabbable)obj.GetComponent(typeof(NearInteractionGrabbable));
                iG.enabled = true;

                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.useGravity = true;
                    rb.constraints = RigidbodyConstraints.FreezeRotation; 
                }

            }
            catch (InvalidCastException e)
            {
                throw new System.MemberAccessException("ObjectCreator:: ApplyConfiguration, not all Components found.", e);
            }

        }
        else if(config == ConfigType.scrollBox)
        {
                if (obj.TryGetComponent(out BoundsControl bC))
                bC.enabled = false;
                if (obj.TryGetComponent(out ObjectManipulator oM))

                oM.enabled = false;
                if (obj.TryGetComponent(out NearInteractionGrabbable iG))

                iG.enabled = false;
                if (obj.TryGetComponent(out Rigidbody rb))
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll; 
                
                    
                if (obj.TryGetComponent(out BoxCollider col))
                    col.enabled = true;
        }
        else
        {
            Debug.LogError("ObjectCreator::ApplyConfiguration wrong configType format.");
        }
    }

    private void ApplyRelevantComponents(GameObject loadedObj)
    {
        loadedObj.tag = "InteractionObject"; 

        // Custom Object Helper
        var helper = loadedObj.EnsureComponent<ObjectHelper>();

        // Rigidbody
        var rb = loadedObj.EnsureComponent<Rigidbody>();
        rb.mass = 1;
        rb.drag = 0;
        rb.angularDrag = 0;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;

        // BoxCollider
        var col = loadedObj.EnsureComponent<BoxCollider>();

        // Audio Source
        var audio = loadedObj.EnsureComponent<AudioSource>();

        // Tethered Placement
        var placementComp = loadedObj.EnsureComponent<TetheredPlacement>();
        placementComp.DistanceThreshold = 20.0f;

        // Near Interaction Grabbable
        var grabComp = loadedObj.EnsureComponent<NearInteractionGrabbable>();
        grabComp.ShowTetherWhenManipulating = false;
        grabComp.IsBoundsHandles = true;

        // ConstraintManager
        var constMan = loadedObj.EnsureComponent<ConstraintManager>();

        // RotationAxisConstraint
        var rotConst = loadedObj.EnsureComponent<RotationAxisConstraint>();
        rotConst.HandType = ManipulationHandFlags.OneHanded;
        rotConst.ConstraintOnRotation = AxisFlags.XAxis;
        rotConst.ConstraintOnRotation = AxisFlags.ZAxis;
        rotConst.UseLocalSpaceForConstraint = true;
        constMan.AddConstraintToManualSelection(rotConst);



        // Min Max Scale Constraint
        var scaleConst = loadedObj.EnsureComponent<MinMaxScaleConstraint>();
            scaleConst.HandType = ManipulationHandFlags.TwoHanded;
            scaleConst.ProximityType = ManipulationProximityFlags.Far;
            scaleConst.ProximityType = ManipulationProximityFlags.Near;
            scaleConst.ScaleMaximum = 1;
            scaleConst.ScaleMinimum = 1;
            scaleConst.RelativeToInitialState = true;

            constMan.AddConstraintToManualSelection(scaleConst);

        // Custom Movement Constraint
        var moveConst = loadedObj.EnsureComponent<CustomMovementConstraint>();
            moveConst.HandType = ManipulationHandFlags.TwoHanded;
            moveConst.UseConstraint = true;
            moveConst.ConstraintOnMovement = AxisFlags.YAxis;

            constMan.AddConstraintToManualSelection(moveConst);

        // Object Manipulator
        var objMan = loadedObj.EnsureComponent<ObjectManipulator>();
            objMan.AllowFarManipulation = false;
            objMan.EnableConstraints = true;
            objMan.ConstraintsManager = constMan;



        // BoundsControl
        var boundsControl = loadedObj.EnsureComponent<BoundsControl>();
            boundsControl.Target = loadedObj;
            boundsControl.BoundsControlActivation = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsControlActivationType.ActivateByProximity;
            boundsControl.BoundsOverride = col;
            boundsControl.CalculationMethod = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsCalculationMethod.RendererOverCollider;

        // Scale Handle
        ScaleHandlesConfiguration config = CreateInstance<ScaleHandlesConfiguration>(); 
            config.ShowScaleHandles = false;  
            boundsControl.ScaleHandlesConfig = config;

        // Translation Handle
        TranslationHandlesConfiguration tConfig = CreateInstance<TranslationHandlesConfiguration>();
        tConfig.ShowHandleForX = false;
            tConfig.ShowHandleForY = false;
            tConfig.ShowHandleForZ = false; 
            boundsControl.TranslationHandlesConfig = tConfig;

        // Rotation Handle
        var rotationHandle = CreateInstance<RotationHandlesConfiguration>();


        // TODO nicht aus resources laden. steht hier: https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/bounds-control?view=mrtkunity-2021-05
        var mat = Resources.Load<Material>(boundingBoxFolderName + "/BoundingBoxHandleWhite");

        if (mat == null)
            throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file found");
        rotationHandle.HandleMaterial = mat;

        var grMat = Resources.Load<Material>(boundingBoxFolderName + "/BoundingBoxHandleBlueGrabbed");
            if (grMat == null)
                throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file found");
            rotationHandle.HandleGrabbedMaterial = grMat;

            var go = Resources.Load<GameObject>(boundingBoxFolderName + "/MRTK_BoundingBox_RotateHandle");
            if (go == null)
                throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file found");
            rotationHandle.HandlePrefab = go;
            
        boundsControl.RotationHandlesConfig = rotationHandle;
        boundsControl.RotationHandlesConfig.ShowHandleForX = false;
        boundsControl.RotationHandlesConfig.ShowHandleForY = true;
        boundsControl.RotationHandlesConfig.ShowHandleForZ = false;

        boundsControl.ConstraintsManager = constMan;

        // Events
        //boundsControl.RotateStarted.AddListener(boundsControl.gameObject.GetComponent<ObjectHelper>().AddMovingObject);    //helper.AddMovingObject);
        //boundsControl.RotateStopped.AddListener(helper.RemoveMovingObject);

        // Events
        objMan.OnManipulationStarted.AddListener(helper.AddObject);
        objMan.OnManipulationEnded.AddListener(helper.RemoveObject);
    }

    private void HandleOnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulated............................."); 

        var fileName = "/MRTK_Manipulation_Start";
        var manStart = Resources.Load<AudioClip>(soundFolderName + fileName);
        if (manStart == null)
            throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file {0} found", fileName);
        eventData.ManipulationSource.GetComponent<AudioSource>().PlayOneShot(manStart); 
    }

    private void HandleOnManipulationStopped(ManipulationEventData eventData)
    {
        Debug.Log("Manipulated.............................");

        var fileName = "/MRTK_Manipulation_End"; 
        var manStop = Resources.Load<AudioClip>(soundFolderName + fileName);
        if (manStop == null)
            throw new FileNotFoundException("... ObjectManager::ApplyRelevantComponents no file {0} found", fileName);
        eventData.ManipulationSource.GetComponent<AudioSource>().PlayOneShot(manStop);
    }





    #endregion private methods

}
