using Stateless;

namespace BugPro;

public sealed class DevelopmentTask
{
    public enum TaskState
    {
        Backlog,
        Analysis,
        Planned,
        InDevelopment,
        CodeReview,
        Testing,
        Fixing,
        ReadyForRelease,
       Done,
        Blocked,
        Cancelled
    }

    public enum TaskTrigger
    {
        StartAnalysis,
        ApproveAnalysis,
        StartDevelopment,
        CompleteDevelopment,
        RequestReview,
        ApproveReview,
        RequestChanges,
        StartTesting,
        PassTests,
        FailTests,
        FixIssue,
        Release,
        Block,
        Unblock,
        Cancel,
        Reopen
    }

    private readonly StateMachine<TaskState, TaskTrigger> _workflow;
    private readonly StateMachine<TaskState, TaskTrigger>.TriggerWithParameters<string> _failTestsTrigger;

    private readonly List<string> _transitionHistory = new();

    public DevelopmentTask()
    {
        _workflow = new StateMachine<TaskState, TaskTrigger>(TaskState.Backlog);
        _failTestsTrigger = _workflow.SetTriggerParameters<string>(TaskTrigger.FailTests);
        
        ConfigureWorkflow();
    }

    public TaskState CurrentState => _workflow.State;
    public IReadOnlyList<string> History => _transitionHistory.AsReadOnly();
    public bool IsCompleted => CurrentState is TaskState.Done or TaskState.Cancelled;

    public void StartAnalysis() => Fire(TaskTrigger.StartAnalysis);
    public void ApproveAnalysis() => Fire(TaskTrigger.ApproveAnalysis);
    public void StartDevelopment() => Fire(TaskTrigger.StartDevelopment);
    public void CompleteDevelopment() => Fire(TaskTrigger.CompleteDevelopment);
    public void RequestReview() => Fire(TaskTrigger.RequestReview);
    public void ApproveReview() => Fire(TaskTrigger.ApproveReview);
    public void RequestChanges() => Fire(TaskTrigger.RequestChanges);
    public void StartTesting() => Fire(TaskTrigger.StartTesting);
    public void PassTests() => Fire(TaskTrigger.PassTests);
    public void FailTests(string reason) => _workflow.Fire(_failTestsTrigger, reason);
    public void FixIssue() => Fire(TaskTrigger.FixIssue);
    public void Release() => Fire(TaskTrigger.Release);
    public void Block() => Fire(TaskTrigger.Block);
    public void Unblock() => Fire(TaskTrigger.Unblock);
    public void Cancel() => Fire(TaskTrigger.Cancel);
    public void Reopen() => Fire(TaskTrigger.Reopen);

    public bool CanTrigger(TaskTrigger trigger) => _workflow.CanFire(trigger);
    
    public string GetReport() => $"Task State: {CurrentState}, Completed: {IsCompleted}, Transitions: {_transitionHistory.Count}";

    public override string ToString() => GetReport();

    private void ConfigureWorkflow()
    {
        _workflow.OnTransitioned(t => 
            _transitionHistory.Add($"[{DateTime.Now:HH:mm:ss}] {t.Source} --{t.Trigger}--> {t.Destination}"));

        ConfigureBacklogState();
        ConfigureAnalysisState();
        ConfigurePlannedState();
        ConfigureInDevelopmentState();
        ConfigureCodeReviewState();
        ConfigureTestingState();
        ConfigureFixingState();
        ConfigureReadyForReleaseState();
        ConfigureDoneState();
        ConfigureBlockedState();
        ConfigureCancelledState();
    }

    private void ConfigureBacklogState()
    {
        _workflow.Configure(TaskState.Backlog)
            .Permit(TaskTrigger.StartAnalysis, TaskState.Analysis)
            .Permit(TaskTrigger.Cancel, TaskState.Cancelled);
    }

    private void ConfigureAnalysisState()
    {
        _workflow.Configure(TaskState.Analysis)
            .Permit(TaskTrigger.ApproveAnalysis, TaskState.Planned)
            .Permit(TaskTrigger.Block, TaskState.Blocked)
            .Permit(TaskTrigger.Cancel, TaskState.Cancelled);
    }

    private void ConfigurePlannedState()
    {
        _workflow.Configure(TaskState.Planned)
            .Permit(TaskTrigger.StartDevelopment, TaskState.InDevelopment)
            .Permit(TaskTrigger.Block, TaskState.Blocked)
            .Permit(TaskTrigger.Cancel, TaskState.Cancelled);
    }

    private void ConfigureInDevelopmentState()
    {
        _workflow.Configure(TaskState.InDevelopment)
            .Permit(TaskTrigger.CompleteDevelopment, TaskState.CodeReview)
            .Permit(TaskTrigger.Block, TaskState.Blocked);
    }

    private void ConfigureCodeReviewState()
    {
        _workflow.Configure(TaskState.CodeReview)
            .Permit(TaskTrigger.ApproveReview, TaskState.Testing)
            .Permit(TaskTrigger.RequestChanges, TaskState.Fixing)
            .Permit(TaskTrigger.Block, TaskState.Blocked);
    }

    private void ConfigureTestingState()
    {
        _workflow.Configure(TaskState.Testing)
            .Permit(TaskTrigger.PassTests, TaskState.ReadyForRelease)
            .Permit(_failTestsTrigger, TaskState.Fixing, reason => !string.IsNullOrEmpty(reason));
    }

    private void ConfigureFixingState()
    {
        _workflow.Configure(TaskState.Fixing)
            .Permit(TaskTrigger.FixIssue, TaskState.CodeReview);
    }

    private void ConfigureReadyForReleaseState()
    {
        _workflow.Configure(TaskState.ReadyForRelease)
            .Permit(TaskTrigger.Release, TaskState.Done)
            .Permit(TaskTrigger.Reopen, TaskState.InDevelopment);
    }

    private void ConfigureDoneState()
    {
        _workflow.Configure(TaskState.Done)
            .Permit(TaskTrigger.Reopen, TaskState.InDevelopment);
    }

    private void ConfigureBlockedState()
    {
        _workflow.Configure(TaskState.Blocked)
            .Permit(TaskTrigger.Unblock, TaskState.Planned);
    }

    private void ConfigureCancelledState()
    {
    }

    private void Fire(TaskTrigger trigger) => _workflow.Fire(trigger);
}