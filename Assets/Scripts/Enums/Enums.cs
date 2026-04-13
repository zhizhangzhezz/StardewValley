public enum AnimationName
{
    idleDown,
    idleUp,
    idleLeft,
    idleRight,
    walkDown,
    walkUp,
    walkLeft,
    walkRight,
    runDown,
    runUp,
    runLeft,
    runRight,
    useToolDown,
    useToolUp,
    useToolLeft,
    useToolRight,
    swingToolDown,
    swingToolUp,
    swingToolLeft,
    swingToolRight,
    liftToolDown,
    liftToolUp,
    liftToolLeft,
    liftToolRight,
    holdToolDown,
    holdToolUp,
    holdToolLeft,
    holdToolRight,
    pickUp,
    pickDown,
    pickLeft,
    pickRight,
    count
}

public enum CharacterPartAnimator
{
    hair,
    body,
    arms,
    tool,
    hat,
    count
}

public enum PartVariantColor
{
    none,
    count
}

public enum PartVariantType
{
    none,
    carry,
    axe,
    hoe,
    pickaxe,
    scythe,
    wateringCan,
    count
}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}

public enum GridBoolProperty
{
    diggable,
    canDropItem,
    canPlaceFurniture,
    isPath,
    isNPCObstacle
}
public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    none,
    count
}

public enum Weather
{
    dry,
    raining,
    snowing,
    none,
    count
}

public enum InventoryLocation
{
    player,
    chest,
    count
}

public enum ToolEffect
{
    none,
    watering
}

public enum Direction
{
    up,
    down,
    left,
    right,
    none
}
//音效
public enum SoundName
{
    none = 0,
    effectFootstepSoftGround = 10,
    effectFootstepHardGround = 20,
    effectAxe = 30,
    effectPickaxe = 40,
    effectcythe = 50,
    effectHoe = 60,
    effectWateringCan = 70,
    effectBasket = 80,
    effectPickupSound = 90,
    effectRustle = 100,
    effectTreeFalling = 110,
    effectPlantingSound = 120,
    effectPlunk = 130,
    effectStoneShatter = 140,
    effectWoodSplinters = 150,
    ambientCountryside1 = 1000,
    ambientCountryside2 = 1010,
    ambientIndoors1 = 1020,
    musicCalm3 = 2000,
    musicCalm1 = 2010
}

public enum ItemType
{
    Seed,
    Commodity,
    WateringTool,
    HoeingTool,
    ChoppingTool,
    BreakingTool,
    ReapingTool,
    CollectingTool,
    Reapable_scenary,
    Furniture,
    none,
    count

}

public enum HarvestActionEffect
{
    deciduousLeavesFalling,
    pineConesFalling,
    choppingTreeTrunk,
    breakingStone,
    reaping,
    none
}
