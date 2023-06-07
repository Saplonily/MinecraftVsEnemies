using NullLib.CommandLine;

namespace MVE;

public partial class Board : Node
{
    public class MiscCmd
    {
        protected readonly Board board;
        protected readonly Lawn lawn;

        public MiscCmd(Board board, Lawn lawn)
        {
            this.board = board;
            this.lawn = lawn;
        }

        [Command(CommandName = "place_weapon")]
        public void PlaceWeapon(int x, int y, string weaponId)
        {
            lawn.PlantingArea.TryPlantAt(new Vector2I(x, y), Game.Instance.WeaponProperties[weaponId], out _);
        }

        [Command(CommandName = "place_enemy")]
        public void PlaceEnemy(int x, int y, string enemyId)
        {
            lawn.PlantingArea.PlaceEnemyAt(new Vector2I(x, y), Game.Instance.EnemyProperties[enemyId]);
        }

        [Command(CommandName = "produce")]
        public void Produce()
        {
            foreach (var node in board.TrackerGet<Furnace>("Furnace"))
            {
                if (node is Furnace furnace)
                {
                    furnace.Produce();
                }
            }
        }

        [Command(CommandName = "set_redstone")]
        public void SetRedstone(double amount)
        {
            board.Bank.AddRedstone(amount - board.Bank.Redstone);
        }

        [Command(CommandName = "kill_all")]
        public void KillAll()
        {
            var enemies = board.GetEnemies();
            foreach (var e in enemies)
                e.Die(DeathReason.Normal);
        }

        [Command(CommandName = "kill_weapon")]
        public void KillWeapon()
        {
            var weapons = board.TrackerGet<Weapon>("Weapon");
            foreach (var wea in weapons)
                wea.Die(DeathReason.Normal);
        }
    }
}
