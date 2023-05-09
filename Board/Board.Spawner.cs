using MVE.SalExt;
using System.Threading;

namespace MVE;


public partial class Board : Node
{
    public enum LevelState
    {
        Invalid,
        OpeningWaiting,
        TravelToSelecting,
        Main,
        Ending,
        Losing
    }

    [Export, ExportGroup("Scenes")] protected PackedScene cardScene = default!;
    [Export] protected PackedScene selectingUIScene = default!;
    [Export] protected PackedScene readySetScene = default!;
    [Export] protected PackedScene hugeWaveTitleScene = default!;
    [Export] protected PackedScene bluePrintScene = default!;

    [Export, ExportGroup("Audios")] protected AudioStream awoogaAudio = default!;

    [Export, ExportGroup("Coordinates")] protected Vector2 cameraStartPos;
    [Export] protected Vector2 cameraBoardPos;
    [Export] protected Vector2 cameraCardSelectingPos;

    protected AudioStreamPlayer awoogaAudioPlayer = default!;
    [Export, ExportGroup("Nodes")] protected Timer waveTimer = default!;
    protected Drop? awardDrop;
    protected SelectingUI selectingUI = default!;
    protected StateMachine<LevelState> stateMachine = default!;

    protected Action<double> updater = default!;

    protected int[] rowWeights = new int[5];


    public LevelState State => stateMachine.State;
    public LevelState InitState { get; set; } = LevelState.OpeningWaiting;

    public int CurrentWave { get; protected set; }
    public LevelData LevelData { get; set; } = default!;
    public bool SpawnerBeginningReadyed { get; protected set; } = false;

    public delegate void BoardWaveChangedEventHandler(Board board, int preWave, int curWave);
    public event BoardWaveChangedEventHandler? WaveChanged;

    public void InitSpawner()
    {
        if (LevelData is null)
        {
            Game.Logger.LogError("LevelLoading", "No LevelData is assigned.");
            throw new Exception("No LevelData is assigned.");
        }
        awoogaAudioPlayer = SalAudioPool.GetPlayer(new(awoogaAudio, Bus: "Board"));

        stateMachine = new(InitState);
        stateMachine.RegisterState(LevelState.OpeningWaiting, LevelOpeningWaiting);
        stateMachine.RegisterState(LevelState.TravelToSelecting, LevelTravelToSelecting);
        stateMachine.RegisterState(LevelState.Main, LevelMain);
        stateMachine.RegisterState(LevelState.Ending);
        stateMachine.RegisterState(LevelState.Losing);
        stateMachine.EnterCurrent();
    }

    #region LevelCoroutine

    public async void LevelOpeningWaiting(LevelState pst)
    {
        CurrentWave = 0;
        boardUIManager.MakeMainHide();
        //开场等待
        await ToSignal(GetTree().CreateTimer(1d), SceneTreeTimer.SignalName.Timeout);

        stateMachine.State = LevelState.TravelToSelecting;
    }

    public async void LevelTravelToSelecting(LevelState pst)
    {
        //开场移动到最右边
        camera.Position = cameraStartPos;
        var cameraTween = camera.CreateTween();
        cameraTween.SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        cameraTween.TweenProperty(camera, "position", cameraCardSelectingPos, 1d);
        await ToSignal(cameraTween, Tween.SignalName.Finished);

        //放置选卡ui
        var finalCards = Game.Instance.SaveData.OwnedCards
            .Union(LevelData.Inventory.CardsAlwaysIncludes)
            .ToList();
        var ui = selectingUIScene.Instantiate<SelectingUI>();
        ui.CardsForSelecting = finalCards;
        boardUIManager.AddChild(ui);
        await ui.StartAndWaitSelecting();

        selectingUI = ui;
        stateMachine.State = LevelState.Main;
    }

    public async void LevelMain(LevelState pst)
    {
        if (pst is LevelState.TravelToSelecting)
        {
            var ui = selectingUI;
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
                c.ChangeDisabledState(true);
                c.Switch2DParent(boardUIManager);
            }

            //移动到版面
            var cameraTween2 = camera.CreateTween();
            cameraTween2.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
            cameraTween2.TweenProperty(camera, "position", cameraBoardPos, 1d);
            await ToSignal(cameraTween2, Tween.SignalName.Finished);

            //销毁所有的ForSelecting的Card并生成并赋值真正的Card
            foreach (var c in selectedCards)
            {
                Vector2 pos = c.Position;
                var s = cardScene.Instantiate<Card>();
                s.Position = pos;
                s.WeaponPropertyId = c.WeaponPropertyId;
                c.Free();
                boardUIManager.AddChild(s);
            }

            boardUIManager.RequestAllDisabledChange(true);
            //显示除卡以外的其他ui
            await boardUIManager.PlayDisplayAnimation();

            await ToSignal(GetTree().CreateTimer(0.25d), SceneTreeTimer.SignalName.Timeout);

            //ready set ui
            ReadySetUI u = readySetScene.Instantiate<ReadySetUI>();
            u.Position = GetViewport().GetVisibleRect().GetCenter();
            layerOverlay.AddChild(u);
            await u.PlayAndFree();
            u.QueueFree();
            boardUIManager.RequestAllDisabledChange(false);
        }

        camera.Position = cameraBoardPos;
        await ToSignal(GetTree().CreateTimer(0.05d), SceneTreeTimer.SignalName.Timeout);

        //切换ui显示模式(从LayerOverlay到LayerMain)
        boardUIManager.Switch2DParent(layerMain);
        layerMain.MoveChild(boardUIManager, -2);

        //刷怪开始
        updater = _ =>
        {
            var enemyCount = GetTree().GetNodesInGroup(GroupNames.Enemy).Count;
            if (enemyCount == 0 && SpawnerBeginningReadyed)
            {
                if (waveTimer.TimeLeft > 0.75d)
                    waveTimer.Start(0.75d);
            }
        };
        SpawnerBeginningReadyed = false;
        waveTimer.Start(50d);
        waveTimer.Timeout += OnSpawnerBeginningReadyed;
    }

    public void OnSpawnerBeginningReadyed()
    {
        waveTimer.Timeout -= OnSpawnerBeginningReadyed;
        SpawnerBeginningReadyed = true;
        awoogaAudioPlayer.Play();
        waveTimer.Timeout += Timeout;
        Timeout();

        void Timeout()
        {
            double time = LevelData.WaveTimerBase + Random.Next1m1Double(LevelData.WaveTimerTemperature);
            waveTimer.Start(time);
            OnMainWave();
            //Game.Logger.LogInfo(Name, $"timeout, cur wave: {CurrentWave}");
        }
    }

    public void OnMainWave()
    {
        if (CurrentWave == LevelData.TotalWaves)
        {
            //Game.Logger.LogInfo(Name, "Next wave of final wave...enter finished state.");
            waveTimer.Stop();
            updater = d =>
            {
                if (!GetEnemies().Any() && awardDrop is null)
                {
                    var award = bluePrintScene.Instantiate<BluePrint>();
                    Lawn.AddBoardEntity(award, Lawn.ToLocal(GetViewport().GetCamera2D().GetScreenCenterPosition()).ToVec3WithZ0());
                    award.ApplyVelocity(new Vector3(Random.Next1m1Float(100, 200), 0, Random.NextFloat(100, 200)));
                    awardDrop = award;
                }
            };
            return;
        }
        CurrentWave += 1;
        WaveChanged?.Invoke(this, CurrentWave - 1, CurrentWave);
        ProcessWave(CurrentWave);
        if (CurrentWave % 10 is 0)
        {
            var s = hugeWaveTitleScene.Instantiate<WaveTitle>();
            s.Position = GetViewport().GetVisibleRect().GetCenter();
            layerOverlay.AddChild(s);
            _ = s.PlayAppear();
            if (CurrentWave == LevelData.TotalWaves)
            {
                //Game.Logger.LogInfo(Name, "Final wave...");
            }
        }
    }

    #endregion

    public void UpdateSpawner(double delta)
        => updater?.Invoke(delta);

    public void ProcessWave(int wave)
    {
        int points = wave * LevelData.EnemiesSpawning.PointsAddFactor;

        if (LevelData.WaveEvent is not null && LevelData.WaveEvent.Events.TryGetValue(CurrentWave.ToString(), out var events))
        {
            foreach (var e in events)
                e.Execute(this, ref points);
        }

        SummonEnemiesBy(LevelData.EnemiesSpawning, points, PlaceEnemy);

    }

    public Enemy PlaceEnemy(in EnemyProperty property, int row)
        => Lawn.PlantingArea.PlaceEnemyAt(new Vector2I(Random.Next(10, 13), row), property);

    public delegate Enemy EnemyPlacingHandler(in EnemyProperty property, int row);

    public void SummonEnemiesBy(in EnemiesSpawningData spawningData, int points, EnemyPlacingHandler placingHandler)
    {
        var leastCost = spawningData.EnemyPool.Min(u => u.Cost);
        if (leastCost > points)
        {
            int row = Random.Next(0, 5);
            var prop = Game.Instance.EnemyProperties["zombie"];
            placingHandler(prop, row);
            var u = spawningData.EnemyPool.FirstOrDefault(u => u.Id == "zombie");
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

            placingHandler(Game.Instance.EnemyProperties[unit.Id], result.ind);
        }
    }
}
