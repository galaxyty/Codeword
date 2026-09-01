public static class SlotUtility
{
    /// <summary>
    /// ÇØ´ç ÀÎµ¦½º°¡ ¾î´ÀÆÀÀÎÁö ¹İÈ¯.
    /// </summary>
    public static ETeam GetIndexTeam(int index, out int slotIndex)
    {
        // È«ÆÀ, Ã»ÆÀ ½½·Ô ÀÎµ¦½º.
        slotIndex = index / 2;

        if (index % 2 == 0)
        {
            return ETeam.Red;
        }

        return ETeam.Blue;
    }
}