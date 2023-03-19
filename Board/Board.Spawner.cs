using MVE.SalExt;

namespace MVE;

public partial class Board : Node
{
    [Export] protected PackedScene cardScene = default!;
    [Export] protected AudioStream awoogaAudio = default!;
    [Export] protected PackedScene selectingUIScene = default!;
    [Export] protected Vector2 cameraStartPos;
    [Export] protected Vector2 cameraBoardPos;
    [Export] protected Vector2 cameraCardSelectingPos;
    protected AudioStreamPlayer awoogaAudioPlayer = default!;
    protected SceneTreeTimer? sceneTreeTimer;

    protected Action<double> updater = default!;

    protected int[] rowWeights = new int[5];

    public int CurrentWave { get; protected set; }
    public LevelData LevelData { get; set; } = default!;
    public bool SpawnerBeginningReadyed { get; protected set; } = false;

    public void InitSpawner()
    {
        awoogaAudioPlayer = SalAudioPool.GetPlayer(new(awoogaAudio, Bus: "Board"));

        CurrentWave = 0;
        LevelCoroutine();
    }

    //TODO 重构为可保存的状态机
    public async void LevelCoroutine()
    {
        boardUIManager.PlayHideAnimation();
        //开场等待
        await ToSignal(GetTree().CreateTimer(1d), SceneTreeTimer.SignalName.Timeout);

        //开场移动到最右边
        camera.Position = cameraStartPos;
        var cameraTween = camera.CreateTween();
        cameraTween.SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        cameraTween.TweenProperty(camera, "position", cameraCardSelectingPos, 1d);
        await ToSignal(cameraTween, Tween.SignalName.Finished);

        //放置选卡ui
        var ui = selectingUIScene.Instantiate<SelectingUI>();
        layerOverlay.AddChild(ui);
        ui.StartAppearAnimation();
        await ToSignal(ui, SelectingUI.SignalName.CardSelectingFinished);

        //选卡结束, 制作选好的卡到boardUI的动画
        var cardTween = CreateTween()
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine)
            .SetParallel();
        var selectedCards = ui.SelectedCardsNode2D.GetChildren().OfType<CardForSelecting>();
        foreach (var c in selectedCards)
        {
            Vector2 targetPos = Extensions.SwitchTransform(ui, boardUIManager, c.Position) + boardUIManager.CardsLayoutStartPos;
            cardTween.TweenProperty(c, "position", targetPos, 0.5);
            c.Switch2DParent(boardUIManager);
        }

        //移动到版面
        var cameraTween2 = camera.CreateTween();
        cameraTween2.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        cameraTween2.TweenProperty(camera, "position", cameraBoardPos, 1d);
        await ToSignal(cameraTween2, Tween.SignalName.Finished);

        //显示除卡以外的其他ui
        boardUIManager.PlayDisplayAnimation();

        //销毁所有的ForSelecting的Card并生成并赋值真正的Card
        foreach (var c in selectedCards)
        {
            Vector2 pos = c.Position;
            var s = cardScene.Instantiate<Card>();
            s.Position = pos;
            s.WeaponPropertyId = c.WeaponPropertyId;
            boardUIManager.AddChild(s);
            c.Free();
        }

        //切换ui显示模式
        boardUIManager.Switch2DParent(layerMain);
        layerMain.MoveChild(boardUIManager, -1);

        //刷怪开始
        updater = _ =>
        {
            var count = GetTree().GetNodesInGroup("Enemy").Count;
            if (count == 0 && SpawnerBeginningReadyed)
            {
                sceneTreeTimer!.TimeLeft = Math.Clamp(sceneTreeTimer!.TimeLeft, 0d, 0.75d);
            }
        };
        SpawnerBeginningReadyed = false;
        sceneTreeTimer = GetTree().CreateTimer(50d);
        await ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
        SpawnerBeginningReadyed = true;
        awoogaAudioPlayer.Play();
        NextWave();
        while (true)
        {
            sceneTreeTimer = GetTree().CreateTimer(Random.NextDouble(10, 14));
            await ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
            NextWave();
        }
    }

    public void UpdateSpawner(double delta)
        => updater?.Invoke(delta);

    public void NextWave()
    {
        CurrentWave += 1;

        int points = CurrentWave * LevelData.EnemiesSpawning.PointsAddFactor;

        SummonEnemiesBy(LevelData.EnemiesSpawning, points, PlaceEnemy);
    }

    public delegate Enemy EnemyPlacingHandler(in EnemyProperty property, int row);

    public void SummonEnemiesBy(EnemiesSpawningData spawningData, int points, EnemyPlacingHandler placingHandler)
    {
        var leastCost = spawningData.EnemyPool.Min(u => u.Cost);
        if (leastCost > points)
        {
            int row = Random.Next(0, 5);
            var prop = Game.Instance.EnemyProperties[0];
            placingHandler(prop, row);
            var u = spawningData.EnemyPool.FirstOrDefault(u => u.InternalId == 0);
            rowWeights[row] -= u is not null ? u.Cost : 100;
            return;
        }

        while (points > 0)
        {
            var unit = spawningData.ChooseUnit(Random, points);
            if (unit is null) break;
            points -= unit.Cost;

            var curMax = rowWeights.Max();
            var allMax = rowWeights.Select((i, ind) => (i, ind)).Where(i => i.i == curMax);
            var allMaxList = allMax.ToList();
            (int i, int ind) result = Chooser<(int, int)>.ChooseFrom(Random, allMaxList);
            rowWeights[result.ind] -= unit.Cost;

            placingHandler(Game.Instance.EnemyProperties[unit.InternalId], result.ind);
        }
    }

    public Enemy PlaceEnemy(in EnemyProperty property, int row)
        => Lawn.PlantingArea.PlaceEnemyAt(new Vector2I(Random.Next(10, 13), row), property);
}
