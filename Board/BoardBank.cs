namespace MVE;

public class BoardBank
{
    /// <summary>
    /// 红石数量变更, 参数为变更量
    /// </summary>
    public event Action<double>? RedstoneChanged;

    public double Redstone { get; protected set; }

    public double AddRedstone(double amount)
    {
        if (amount >= 0d)
        {
            Redstone += amount;
            RedstoneChanged?.Invoke(amount);
        }
        else
        {
            Game.Logger.LogWarn("BoardBank/AddRedstone", "Try to add <0 amount redstone.");
        }
        return Redstone;
    }

    public double ReduceRedstone(double amount)
    {
        if (amount >= 0d)
        {
            Redstone -= amount;
            RedstoneChanged?.Invoke(-amount);
        }
        else
        {
            Game.Logger.LogWarn("BoardBank/AddRedstone", "Try to reduce <0 amount redstone.");
        }
        return Redstone;
    }

    public double SetRedstone(double targetAmount)
    {
        double diff = targetAmount - Redstone;
        Redstone = targetAmount;
        RedstoneChanged?.Invoke(diff);
        return Redstone;
    }
}
