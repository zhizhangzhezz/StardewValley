[System.Serializable]
//角色属性
public struct CharacterAttribute
{
    public CharacterPartAnimator characterPart;
    public PartVariantType partVariantType;
    public PartVariantColor partVariantColor;

    public CharacterAttribute(CharacterPartAnimator characterPart, PartVariantType partVariantType, PartVariantColor partVariantColor)
    {
        this.characterPart = characterPart;
        this.partVariantType = partVariantType;
        this.partVariantColor = partVariantColor;
    }
}
