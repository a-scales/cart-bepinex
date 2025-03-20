## Cart Summary
- **Basic Identification**: Medium-sized cart with physics-based interactions
- **Control Methods**:
  - Basic grabbing via PhysGrabObject component
  - Enhanced control through dedicated Cart Handle and Cart Grab Point
  - State system (currently "Locked") controls behavior modes
- **Item Management**:
  - PhysGrabInCart component tracks items inside the cart
  - Collider system keeps items contained
  - Display shows current haul value
- **Physics Properties**:
  - Mass: 2, Drag: 2, Angular Drag: 4
  - Multiple specialized physics materials for different situations
  - Stabilization force (100) helps maintain balance
  - Three capsule colliders configured as wheels for movement
- **Visual Feedback**:
  - Screen with TextMeshPro display shows information
  - Handle system provides visual indicators for interaction
  - Particle effects for impacts and movement

[Info   :BecomeCart] Cart: Level Generator/Items/Item Cart Medium(Clone)
[Info   :BecomeCart] GameObject: Item Cart Medium(Clone), Active: True, Layer: PhysGrabObject
[Info   :BecomeCart]   Component: Transform
[Info   :BecomeCart]   Component: PhotonView
[Info   :BecomeCart]   Component: ItemAttributes
[Info   :BecomeCart]   Component: PhysGrabObject
[Info   :BecomeCart] === Fields for PhysGrabObject ===
[Info   :BecomeCart]   Single alterAngularDragValue = 0
[Info   :BecomeCart]   Single alterDragValue = 0
[Info   :BecomeCart]   Single alterMassValue = 0
[Info   :BecomeCart]   PhysicMaterial alterMaterialCurrent = null
[Info   :BecomeCart]   PhysicMaterial alterMaterialPrevious = null
[Info   :BecomeCart]   Single angle = 0
[Info   :BecomeCart]   Single angularDragOriginal = 4
[Info   :BecomeCart]   Vector3 boundingBox = (1.25, 1.05, 2.27)
[Info   :BecomeCart]   Single breakHeavyTimer = 0
[Info   :BecomeCart]   Single breakLightTimer = 0
[Info   :BecomeCart]   Single breakMediumTimer = 0
[Info   :BecomeCart]   Vector3 camRelForward = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 camRelUp = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 centerPoint = (-0.94, 0.77, -6.72)
[Info   :BecomeCart]   Boolean clientNonKinematic = False
[Info   :BecomeCart]   List`1 colliders = System.Collections.Generic.List`1[UnityEngine.Transform]
[Info   :BecomeCart]   Vector3 currentPosition = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Quaternion currentRotation = (0.00000, 0.00000, 0.00000, 0.00000)
[Info   :BecomeCart]   Boolean dead = False
[Info   :BecomeCart]   Single distance = 0
[Info   :BecomeCart]   Single dragOriginal = 2
[Info   :BecomeCart]   Single enemyInteractTimer = 0
[Info   :BecomeCart]   Transform forceGrabPoint = null
[Info   :BecomeCart]   Boolean frozen = False
[Info   :BecomeCart]   Vector3 frozenAngularVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 frozenForce = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 frozenPosition = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Quaternion frozenRotation = (0.00000, 0.00000, 0.00000, 0.00000)
[Info   :BecomeCart]   Single frozenTimer = 0
[Info   :BecomeCart]   Vector3 frozenTorque = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 frozenVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Boolean grabbed = False
[Info   :BecomeCart]   Boolean grabbedLocal = False
[Info   :BecomeCart]   Single grabbedTimer = 0
[Info   :BecomeCart]   Single grabDisableTimer = 0
[Info   :BecomeCart]   Vector3 grabDisplacementCurrent = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 grabRotation = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Single gradualLerp = 0
[Info   :BecomeCart]   Boolean hasHinge = False
[Info   :BecomeCart]   Boolean hasImpactDetector = True
[Info   :BecomeCart]   Boolean hasMapCustom = True
[Info   :BecomeCart]   Boolean hasNeverBeenGrabbed = True
[Info   :BecomeCart]   Boolean heavyBreakImpulse = False
[Info   :BecomeCart]   Boolean heavyImpactImpulse = False
[Info   :BecomeCart]   Boolean heldByLocalPlayer = False
[Info   :BecomeCart]   PhysGrabHinge hinge = null
[Info   :BecomeCart]   PhysGrabObjectImpactDetector impactDetector = Item Cart Medium(Clone) (PhysGrabObjectImpactDetector)
[Info   :BecomeCart]   Single impactHappenedTimer = 0
[Info   :BecomeCart]   Single impactHeavyTimer = 0
[Info   :BecomeCart]   Single impactLightTimer = 0
[Info   :BecomeCart]   Single impactMediumTimer = 0
[Info   :BecomeCart]   Boolean isActive = True
[Info   :BecomeCart]   Boolean isCart = True
[Info   :BecomeCart]   Boolean isEnemy = False
[Info   :BecomeCart]   Boolean isHidden = False
[Info   :BecomeCart]   Boolean isKinematic = False
[Info   :BecomeCart]   Boolean isMaster = False
[Info   :BecomeCart]   Boolean isMelee = False
[Info   :BecomeCart]   Boolean isNonValuable = True
[Info   :BecomeCart]   Boolean isPlayer = False
[Info   :BecomeCart]   Boolean isValuable = False
[Info   :BecomeCart]   PlayerAvatar lastPlayerGrabbing = null
[Info   :BecomeCart]   Single lastUpdateTime = 0
[Info   :BecomeCart]   Boolean lightBreakImpulse = False
[Info   :BecomeCart]   Boolean lightImpactImpulse = False
[Info   :BecomeCart]   Camera mainCamera = Camera Main (UnityEngine.Camera)
[Info   :BecomeCart]   MapCustom mapCustom = Item Cart Medium(Clone) (MapCustom)
[Info   :BecomeCart]   Single massOriginal = 2
[Info   :BecomeCart]   Boolean mediumBreakImpulse = False
[Info   :BecomeCart]   Boolean mediumImpactImpulse = False
[Info   :BecomeCart]   Vector3 midPoint = (-0.94, 0.77, -6.72)
[Info   :BecomeCart]   Vector3 midPointOffset = (0.00, 0.02, 0.00)
[Info   :BecomeCart]   Single overrideAngularDragGoDownTimer = 0
[Info   :BecomeCart]   Single overrideDisableBreakEffectsTimer = 0
[Info   :BecomeCart]   Single overrideDragGoDownTimer = 0
[Info   :BecomeCart]   Single overrideFragilityTimer = -123
[Info   :BecomeCart]   Single overrideGrabRelativeHorizontalPosition = 0
[Info   :BecomeCart]   Single overrideGrabRelativeHorizontalPositionTimer = 0
[Info   :BecomeCart]   Single overrideGrabRelativeVerticalPosition = 0
[Info   :BecomeCart]   Single overrideGrabRelativeVerticalPositionTimer = 0
[Info   :BecomeCart]   Single overrideGrabStrength = 1
[Info   :BecomeCart]   Single overrideGrabStrengthTimer = 0
[Info   :BecomeCart]   Single overrideMassGoDownTimer = 0
[Info   :BecomeCart]   Boolean overrideTagsAndLayers = True
[Info   :BecomeCart]   Single overrideTorqueStrength = 1
[Info   :BecomeCart]   Single overrideTorqueStrengthTimer = 0
[Info   :BecomeCart]   Single overrideTorqueStrengthX = 1
[Info   :BecomeCart]   Single overrideTorqueStrengthXTimer = 0
[Info   :BecomeCart]   Single overrideTorqueStrengthY = 1
[Info   :BecomeCart]   Single overrideTorqueStrengthYTimer = 0
[Info   :BecomeCart]   Single overrideTorqueStrengthZ = 1
[Info   :BecomeCart]   Single overrideTorqueStrengthZTimer = 0
[Info   :BecomeCart]   PhotonTransformView photonTransformView = null
[Info   :BecomeCart]   PhotonView photonView = View 0 on Item Cart Medium(Clone) (scene)
[Info   :BecomeCart]   PhysGrabCart physGrabCart = Item Cart Medium(Clone) (PhysGrabCart)
[Info   :BecomeCart]   List`1 playerGrabbing = System.Collections.Generic.List`1[PhysGrabber]
[Info   :BecomeCart]   List`1 positionBuffer = System.Collections.Generic.List`1[System.ValueTuple`2[UnityEngine.Vector3,System.Double]]
[Info   :BecomeCart]   CollisionDetectionMode previousCollisionDetectionMode = Discrete
[Info   :BecomeCart]   Vector3 prevTargetPos = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Quaternion prevTargetRot = (0.00000, 0.00000, 0.00000, 0.00000)
[Info   :BecomeCart]   Rigidbody rb = Item Cart Medium(Clone) (UnityEngine.Rigidbody)
[Info   :BecomeCart]   Vector3 rbAngularVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Boolean rbIsSleepingPrevious = False
[Info   :BecomeCart]   Vector3 rbVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   RoomVolumeCheck roomVolumeCheck = Item Cart Medium(Clone) (RoomVolumeCheck)
[Info   :BecomeCart]   List`1 rotationBuffer = System.Collections.Generic.List`1[System.ValueTuple`2[UnityEngine.Quaternion,System.Double]]
[Info   :BecomeCart]   Single smoothRotationDelta = 0
[Info   :BecomeCart]   Boolean spawned = True
[Info   :BecomeCart]   Vector3 spawnTorque = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 targetPos = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Quaternion targetRot = (0.00000, 0.00000, 0.00000, 0.00000)
[Info   :BecomeCart]   Single timerAlterAngularDrag = -123
[Info   :BecomeCart]   Single timerAlterDeactivate = -123
[Info   :BecomeCart]   Single timerAlterDrag = -123
[Info   :BecomeCart]   Single timerAlterIndestructible = -123
[Info   :BecomeCart]   Single timerAlterMass = -123
[Info   :BecomeCart]   Single timerAlterMaterial = -123
[Info   :BecomeCart]   Single timerZeroGravity = -123
[Info   :BecomeCart]   Component: NotValuableObject
[Info   :BecomeCart]   Component: RoomVolumeCheck
[Info   :BecomeCart]   Component: Rigidbody
[Info   :BecomeCart]     Rigidbody: mass=2, drag=2, useGravity=True
[Info   :BecomeCart]     Velocity: (0.00, 0.00, 0.00), AngularVelocity: (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Component: PhysGrabObjectImpactDetector
[Info   :BecomeCart] === Fields for PhysGrabObjectImpactDetector ===
[Info   :BecomeCart]   Boolean audioActive = True
[Info   :BecomeCart]   Single breakForce = 0
[Info   :BecomeCart]   Single breakLevel1Cooldown = 0
[Info   :BecomeCart]   Single breakLevel2Cooldown = 0
[Info   :BecomeCart]   Single breakLevel3Cooldown = 0
[Info   :BecomeCart]   Int32 breakLevelHeavy = 0
[Info   :BecomeCart]   Int32 breakLevelLight = 2
[Info   :BecomeCart]   Int32 breakLevelMedium = 1
[Info   :BecomeCart]   Boolean breakLogic = False
[Info   :BecomeCart]   Boolean canHurtLogic = True
[Info   :BecomeCart]   PhysGrabCart cart = Item Cart Medium(Clone) (PhysGrabCart)
[Info   :BecomeCart]   List`1 colliderTransforms = System.Collections.Generic.List`1[UnityEngine.Transform]
[Info   :BecomeCart]   Single colliderVolume = 35.60947
[Info   :BecomeCart]   Single collisionActivatedBuffer = 0
[Info   :BecomeCart]   Boolean collisionsActive = False
[Info   :BecomeCart]   Single collisionsActiveTimer = 0
[Info   :BecomeCart]   Vector3 contactPoint = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   PhysGrabCart currentCart = null
[Info   :BecomeCart]   Boolean destroyDisable = False
[Info   :BecomeCart]   Int32 destroyDisableLaunches = 0
[Info   :BecomeCart]   Single destroyDisableLaunchesTimer = 0
[Info   :BecomeCart]   Boolean destroyDisableTeleport = True
[Info   :BecomeCart]   Single durability = 100
[Info   :BecomeCart]   Single enemyInteractionTimer = 0
[Info   :BecomeCart]   EnemyRigidbody enemyRigidbody = null
[Info   :BecomeCart]   Single fragility = 50
[Info   :BecomeCart]   Single fragilityMultiplier = 1
[Info   :BecomeCart]   Single highestVelocity = 0
[Info   :BecomeCart]   PhysAudio impactAudio = PhysAudio - Cart (PhysAudio)
[Info   :BecomeCart]   Single impactAudioPitch = 1
[Info   :BecomeCart]   Single impactCheckTimer = 0
[Info   :BecomeCart]   Single impactCooldown = 0
[Info   :BecomeCart]   Single impactDisabledTimer = 0
[Info   :BecomeCart]   Single impactForce = 0
[Info   :BecomeCart]   Single impactFragilityMultiplier = 1
[Info   :BecomeCart]   Boolean impactHappened = False
[Info   :BecomeCart]   Single impactHeavyCooldown = 0
[Info   :BecomeCart]   Single impactLevel1 = 300
[Info   :BecomeCart]   Single impactLevel2 = 400
[Info   :BecomeCart]   Single impactLevel3 = 500
[Info   :BecomeCart]   Single impactLightCooldown = 0
[Info   :BecomeCart]   Single impactMediumCooldown = 0
[Info   :BecomeCart]   Single impulseTimerDeactivateImpacts = -0.003191475
[Info   :BecomeCart]   Boolean inCart = False
[Info   :BecomeCart]   Boolean inCartPrevious = False
[Info   :BecomeCart]   Single inCartVolumeMultiplier = 0.6
[Info   :BecomeCart]   Boolean indestructibleBreakEffects = True
[Info   :BecomeCart]   Single indestructibleSpawnTimer = -0.003191475
[Info   :BecomeCart]   Boolean isBrokenHinge = False
[Info   :BecomeCart]   Boolean isCart = True
[Info   :BecomeCart]   Boolean isColliding = False
[Info   :BecomeCart]   Single isCollidingTimer = 0
[Info   :BecomeCart]   Boolean isEnemy = False
[Info   :BecomeCart]   Boolean isHinge = False
[Info   :BecomeCart]   Boolean isIndestructible = False
[Info   :BecomeCart]   Boolean isMoving = True
[Info   :BecomeCart]   Boolean isNotValuable = True
[Info   :BecomeCart]   Boolean isSliding = False
[Info   :BecomeCart]   Boolean isValuable = False
[Info   :BecomeCart]   Camera mainCamera = Camera Main (UnityEngine.Camera)
[Info   :BecomeCart]   MaterialTrigger materialTrigger = Materials+MaterialTrigger
[Info   :BecomeCart]   NotValuableObject notValuableObject = Item Cart Medium(Clone) (NotValuableObject)
[Info   :BecomeCart]   UnityEvent onAllBreaks = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onAllImpacts = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onBreakHeavy = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onBreakLight = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onBreakMedium = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onDestroy = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onImpactHeavy = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onImpactLight = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   UnityEvent onImpactMedium = UnityEngine.Events.UnityEvent UnityEngine.Events.UnityEvent
[Info   :BecomeCart]   Vector3 originalPosition = (-0.94, 0.65, -6.74)
[Info   :BecomeCart]   Quaternion originalRotation = (0.00000, 0.00000, 0.00000, 1.00000)
[Info   :BecomeCart]   Boolean particleDisable = False
[Info   :BecomeCart]   Single particleMultiplier = 1
[Info   :BecomeCart]   PhysObjectParticles particles = Phys Object Particles(Clone) (PhysObjectParticles)
[Info   :BecomeCart]   PhotonView photonView = View 0 on Item Cart Medium(Clone) (scene)
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]   Boolean playerHurtDisable = True
[Info   :BecomeCart]   Single playerHurtMultiplier = 1
[Info   :BecomeCart]   Single playerHurtMultiplierTimer = 0
[Info   :BecomeCart]   Vector3 previousAngularVelocity = (0.00, 0.01, 0.00)
[Info   :BecomeCart]   Vector3 previousPosition = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 previousPreviousVelocityRaw = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 previousRotation = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 previousSlidingPosition = (-0.94, 0.75, -6.72)
[Info   :BecomeCart]   Vector3 previousVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 previousVelocityRaw = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 prevPos = (-0.94, 0.75, -6.72)
[Info   :BecomeCart]   Quaternion prevRot = (0.00000, 0.00115, 0.00000, -1.00000)
[Info   :BecomeCart]   Rigidbody rb = Item Cart Medium(Clone) (UnityEngine.Rigidbody)
[Info   :BecomeCart]   Single resetPrevPositionTimer = 0
[Info   :BecomeCart]   Single slidingAudioSpeed = 0
[Info   :BecomeCart]   Boolean slidingDisable = False
[Info   :BecomeCart]   Single slidingGain = 0
[Info   :BecomeCart]   Single slidingSpeedThreshold = 0.1
[Info   :BecomeCart]   Single slidingTimer = 0
[Info   :BecomeCart]   Single timerInCart = 0
[Info   :BecomeCart]   ValuableObject valuableObject = null
[Info   :BecomeCart]   Component: PhysGrabCart
[Info   :BecomeCart] === Fields for PhysGrabCart ===
[Info   :BecomeCart]   Vector3 actualVelocity = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Vector3 actualVelocityLastPosition = (-0.94, 0.75, -6.72)
[Info   :BecomeCart]   Single autoTurnOffTimer = 0
[Info   :BecomeCart]   GameObject buttonObject = null
[Info   :BecomeCart]   List`1 capsuleColliders = System.Collections.Generic.List`1[UnityEngine.Collider]
[Info   :BecomeCart]   Boolean cartActive = False
[Info   :BecomeCart]   Boolean cartActivePrevious = False
[Info   :BecomeCart]   Boolean cartBeingPulled = False
[Info   :BecomeCart]   Transform cartGrabPoint = Cart Grab Point (UnityEngine.Transform)
[Info   :BecomeCart]   List`1 cartInside = System.Collections.Generic.List`1[UnityEngine.Collider]
[Info   :BecomeCart]   MeshRenderer cartMesh = Cart Mesh (UnityEngine.MeshRenderer)
[Info   :BecomeCart]   State currentState = Locked
[Info   :BecomeCart]   Boolean deductedFromHaul = False
[Info   :BecomeCart]   TextMeshPro displayText = Text (TMPro.TextMeshPro)
[Info   :BecomeCart]   Single draggedTimer = 0
[Info   :BecomeCart]   List`1 grabMaterial = System.Collections.Generic.List`1[UnityEngine.Material]
[Info   :BecomeCart]   MeshRenderer[] grabMesh = UnityEngine.MeshRenderer[]
[Info   :BecomeCart]   Transform handlePoint = null
[Info   :BecomeCart]   Int32 haulCurrent = 0
[Info   :BecomeCart]   Int32 haulPrevious = 0
[Info   :BecomeCart]   Single haulUpdateEffectTimer = 0
[Info   :BecomeCart]   Vector3 hitPoint = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Transform inCart = In Cart (UnityEngine.Transform)
[Info   :BecomeCart]   Boolean isSmallCart = False
[Info   :BecomeCart]   ItemEquippable itemEquippable = null
[Info   :BecomeCart]   List`1 itemsInCart = System.Collections.Generic.List`1[PhysGrabObject]
[Info   :BecomeCart]   Int32 itemsInCartCount = 0
[Info   :BecomeCart]   Vector3 lastPosition = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Single objectInCartCheckTimer = 0.329493
[Info   :BecomeCart]   Color originalHaulColor = RGBA(1.000, 0.553, 0.000, 1.000)
[Info   :BecomeCart]   PhotonView photonView = View 0 on Item Cart Medium(Clone) (scene)
[Info   :BecomeCart]   PhysGrabInCart physGrabInCart = In Cart (PhysGrabInCart)
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]   PhysGrabObjectGrabArea physGrabObjectGrabArea = Item Cart Medium(Clone) (PhysGrabObjectGrabArea)
[Info   :BecomeCart]   PhysicMaterial physMaterialALilSlippery = ALilSlippery (UnityEngine.PhysicMaterial)
[Info   :BecomeCart]   PhysicMaterial physMaterialNormal = Default (UnityEngine.PhysicMaterial)
[Info   :BecomeCart]   PhysicMaterial physMaterialSlippery = ExtremelySlippery (UnityEngine.PhysicMaterial)
[Info   :BecomeCart]   PhysicMaterial physMaterialSticky = Extremely NON Slippery (UnityEngine.PhysicMaterial)
[Info   :BecomeCart]   Single playerInteractionTimer = 0
[Info   :BecomeCart]   State previousState = Locked
[Info   :BecomeCart]   Rigidbody rb = Item Cart Medium(Clone) (UnityEngine.Rigidbody)
[Info   :BecomeCart]   Boolean resetHaulText = True
[Info   :BecomeCart]   GameObject smallCartHurtCollider = null
[Info   :BecomeCart]   Sound soundDragged = Sound
[Info   :BecomeCart]   Sound soundHandled = Sound
[Info   :BecomeCart]   Sound soundHaulDecrease = Sound
[Info   :BecomeCart]   Sound soundHaulIncrease = Sound
[Info   :BecomeCart]   Sound soundLocked = Sound
[Info   :BecomeCart]   Single stabilizationForce = 100
[Info   :BecomeCart]   Boolean thirtyFPSUpdate = False
[Info   :BecomeCart]   Single thirtyFPSUpdateTimer = 0.02776024
[Info   :BecomeCart]   Vector3 velocityRef = (0.00, 0.00, 0.00)
[Info   :BecomeCart]   Component: MapCustom
[Info   :BecomeCart]   Component: PhysGrabObjectGrabArea
[Info   :BecomeCart] === Fields for PhysGrabObjectGrabArea ===
[Info   :BecomeCart]   List`1 grabAreas = System.Collections.Generic.List`1[PhysGrabObjectGrabArea+GrabArea]
[Info   :BecomeCart]   List`1 listOfAllGrabbers = System.Collections.Generic.List`1[PhysGrabber]
[Info   :BecomeCart]   PhotonView photonView = View 0 on Item Cart Medium(Clone) (scene)
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]   StaticGrabObject staticGrabObject = null
[Info   :BecomeCart]   Component: PhotonTransformView
[Info   :BecomeCart]   Component: Boombox
[Info   :BecomeCart]   Component: AudioSource
[Info   :BecomeCart]   Component: AudioLowPassFilter
[Info   :BecomeCart]   Component: BoomboxController
[Info   :BecomeCart]   Children (17):
[Info   :BecomeCart]   GameObject: Screen, Active: True, Layer: PhysGrabObject
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Children (4):
[Info   :BecomeCart]     GameObject: Cube (13), Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Screen Black (Instance), shader: Particles/Standard Unlit
[Info   :BecomeCart]     GameObject: Text, Active: True, Layer: Default
[Info   :BecomeCart]       Component: RectTransform
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Teko-VariableFont_wght SDF_SCREEN (Instance), shader: TextMeshPro/Distance Field (Surface)
[Info   :BecomeCart]       Component: TextMeshPro
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]     GameObject: Display Wide, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Displays and Buttons (Instance), shader: Standard
[Info   :BecomeCart]       Children (1):
[Info   :BecomeCart]       GameObject: Display Wide Holder, Active: True, Layer: Default
[Info   :BecomeCart]         Component: Transform
[Info   :BecomeCart]         Component: MeshFilter
[Info   :BecomeCart]         Component: MeshRenderer
[Info   :BecomeCart]           MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]             Material: Displays and Buttons (Instance), shader: Standard
[Info   :BecomeCart]     GameObject: Screen Light, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: Light
[Info   :BecomeCart]       Component: PropLight
[Info   :BecomeCart]   GameObject: Capsule Left, Active: True, Layer: CartWheels
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: CapsuleCollider
[Info   :BecomeCart]       Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]       CapsuleCollider radius: 0.5, height: 2
[Info   :BecomeCart]     Component: GizmoCapsule
[Info   :BecomeCart]   GameObject: Capsule Mid, Active: True, Layer: CartWheels
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: CapsuleCollider
[Info   :BecomeCart]       Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]       CapsuleCollider radius: 0.5, height: 2
[Info   :BecomeCart]     Component: GizmoCapsule
[Info   :BecomeCart]   GameObject: Capsule Right, Active: True, Layer: CartWheels
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: CapsuleCollider
[Info   :BecomeCart]       Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]       CapsuleCollider radius: 0.5, height: 2
[Info   :BecomeCart]     Component: GizmoCapsule
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 0
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 1
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 2
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 3
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 4
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Cart Wall Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 2 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 5
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: In Cart, Active: True, Layer: PhysGrabObjectTrigger
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: BoxCollider
[Info   :BecomeCart]       Collider: isTrigger=True, enabled=True
[Info   :BecomeCart]       BoxCollider size: (1.00, 1.00, 1.84), center: (0.00, 0.00, -0.42)
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: PhysGrabInCart
[Info   :BecomeCart] === Fields for PhysGrabInCart ===
[Info   :BecomeCart]   PhysGrabCart cart = Item Cart Medium(Clone) (PhysGrabCart)
[Info   :BecomeCart]   List`1 inCartObjects = System.Collections.Generic.List`1[PhysGrabInCart+CartObject]
[Info   :BecomeCart]   GameObject: Inside, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: New Material 1 (Instance), shader: Standard
[Info   :BecomeCart]     Children (1):
[Info   :BecomeCart]     GameObject: Semi Box Collider, Active: True, Layer: PhysGrabObjectCart
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: BoxCollider
[Info   :BecomeCart]         Collider: isTrigger=False, enabled=True
[Info   :BecomeCart]         BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]       Component: PhysGrabObjectBoxCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectBoxCollider ===
[Info   :BecomeCart]   Boolean drawGizmos = True
[Info   :BecomeCart]   Single gizmoTransparency = 1
[Info   :BecomeCart]   Boolean unEquipCollider = False
[Info   :BecomeCart]       Component: PhysGrabObjectCollider
[Info   :BecomeCart] === Fields for PhysGrabObjectCollider ===
[Info   :BecomeCart]   Int32 colliderID = 6
[Info   :BecomeCart]   PhysGrabObject physGrabObject = Item Cart Medium(Clone) (PhysGrabObject)
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]   GameObject: Grab Area, Active: True, Layer: Ignore Raycast
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: BoxCollider
[Info   :BecomeCart]       Collider: isTrigger=True, enabled=True
[Info   :BecomeCart]       BoxCollider size: (1.00, 1.00, 1.00), center: (0.00, 0.00, 0.00)
[Info   :BecomeCart]     Component: DrawGizmoCube
[Info   :BecomeCart]   GameObject: Cart Handle, Active: True, Layer: PhysGrabObject
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=False, materialCount=1
[Info   :BecomeCart]         Material: Cart Handle (Instance), shader: Standard
[Info   :BecomeCart]     Children (3):
[Info   :BecomeCart]     GameObject: Grab, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Grab Arrow (Instance), shader: Standard
[Info   :BecomeCart]     GameObject: Grab (1), Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Grab Switch Material (Instance), shader: Standard
[Info   :BecomeCart]     GameObject: Grab (2), Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Grab Arrow (Instance), shader: Standard
[Info   :BecomeCart]   GameObject: Cart Mesh, Active: True, Layer: PhysGrabObject
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: MeshFilter
[Info   :BecomeCart]     Component: MeshRenderer
[Info   :BecomeCart]       MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]         Material: Cart Medium (Instance), shader: Standard
[Info   :BecomeCart]     Children (2):
[Info   :BecomeCart]     GameObject: GameObject (1), Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Cart Medium (Instance), shader: Standard
[Info   :BecomeCart]     GameObject: GameObject (2), Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: MeshFilter
[Info   :BecomeCart]       Component: MeshRenderer
[Info   :BecomeCart]         MeshRenderer: isVisible=True, materialCount=1
[Info   :BecomeCart]           Material: Cart Medium (Instance), shader: Standard
[Info   :BecomeCart]   GameObject: Cart Grab Point, Active: True, Layer: PhysGrabObject
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]   GameObject: Phys Object Particles(Clone), Active: True, Layer: Default
[Info   :BecomeCart]     Component: Transform
[Info   :BecomeCart]     Component: PhysObjectParticles
[Info   :BecomeCart]     Children (3):
[Info   :BecomeCart]     GameObject: Particle System Bits, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: ParticleSystem
[Info   :BecomeCart]       Component: ParticleSystemRenderer
[Info   :BecomeCart]     GameObject: Particle System Bits Small, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: ParticleSystem
[Info   :BecomeCart]       Component: ParticleSystemRenderer
[Info   :BecomeCart]     GameObject: Particle System Smoke, Active: True, Layer: Default
[Info   :BecomeCart]       Component: Transform
[Info   :BecomeCart]       Component: ParticleSystemRenderer
[Info   :BecomeCart]       Component: ParticleSystem