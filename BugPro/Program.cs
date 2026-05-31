using Stateless;

namespace IssueTracker;

public sealed class Issue
{
    private readonly List<string> _log = [];

    public enum Status
    {
        Created,
        Reviewed,
        Active,
        AwaitingResponse,
        Postponed,
        Fixed,
        Verified,
        ReopenedState,
        Declined,
        DuplicateEntry,
        Unreproducible
    }

    public enum Action
    {
        Review,
        StartWork,
        AskQuestion,
        AnswerQuestion,
        Postpone,
        Continue,
        MarkFixed,
        ValidateFix,
        ReopenIssue,
        ConfirmClose,
        RejectIssue,
        MarkAsDuplicate,
        MarkUnreproducible,
        SendBackForReview
    }

    private readonly StateMachine<Status, Action> _machine;
    private readonly StateMachine<Status, Action>.TriggerWithParameters<bool> _validateTrigger;

    public Issue()
    {
        _machine = new StateMachine<Status, Action>(Status.Created);
        _validateTrigger = _machine.SetTriggerParameters<bool>(Action.ValidateFix);

        SetupTransitions();
    }

    public IReadOnlyList<string> EventLog => _log.AsReadOnly();
    public Status CurrentStatus => _machine.State;

    public bool CanExecute(Action action) => _machine.CanFire(action);
    
    public bool IsTerminal => CurrentStatus is Status.Verified or Status.Declined or Status.DuplicateEntry;

    public void PerformReview() => Execute(Action.Review);
    public void StartWorking() => Execute(Action.StartWork);
    public void RequestDetails() => Execute(Action.AskQuestion);
    public void SupplyDetails() => Execute(Action.AnswerQuestion);
    public void SetPostponed() => Execute(Action.Postpone);
    public void ResumeWork() => Execute(Action.Continue);
    public void MarkAsFixed() => Execute(Action.MarkFixed);
    public void ValidateFix(bool isResolved) => _machine.Fire(_validateTrigger, isResolved);
    public void ReopenIssue() => Execute(Action.ReopenIssue);
    public void FinalizeClose() => Execute(Action.ConfirmClose);
    public void MarkAsRejected() => Execute(Action.RejectIssue);
    public void MarkAsDuplicate() => Execute(Action.MarkAsDuplicate);
    public void MarkUnreproducible() => Execute(Action.MarkUnreproducible);
    public void ReturnForReview() => Execute(Action.SendBackForReview);

    public string GenerateReport() =>
        $"Issue status: {CurrentStatus}; terminal: {IsTerminal}; log entries: {EventLog.Count}";

    public override string ToString() => GenerateReport();

    private void SetupTransitions()
    {
        TrackTransitions();
        ConfigureCreatedState();
        ConfigureReviewedState();
        ConfigureActiveState();
        ConfigureAwaitingState();
        ConfigurePostponedState();
        ConfigureFixedState();
        ConfigureTerminalStates();
        ConfigureReopenedState();
    }

    private void TrackTransitions()
    {
        _machine.OnTransitioned(transition => _log.Add(FormatTransition(transition)));
    }

    private void ConfigureCreatedState()
    {
        _machine.Configure(Status.Created)
            .Permit(Action.Review, Status.Reviewed);
    }

    private void ConfigureReviewedState()
    {
        _machine.Configure(Status.Reviewed)
            .Permit(Action.StartWork, Status.Active)
            .Permit(Action.AskQuestion, Status.AwaitingResponse)
            .Permit(Action.Postpone, Status.Postponed)
            .Permit(Action.RejectIssue, Status.Declined)
            .Permit(Action.MarkAsDuplicate, Status.DuplicateEntry)
            .Permit(Action.MarkUnreproducible, Status.Unreproducible);
    }

    private void ConfigureActiveState()
    {
        _machine.Configure(Status.Active)
            .Permit(Action.AskQuestion, Status.AwaitingResponse)
            .Permit(Action.Postpone, Status.Postponed)
            .Permit(Action.MarkFixed, Status.Fixed);
    }

    private void ConfigureAwaitingState()
    {
        _machine.Configure(Status.AwaitingResponse)
            .Permit(Action.AnswerQuestion, Status.Reviewed)
            .Permit(Action.StartWork, Status.Active);
    }

    private void ConfigurePostponedState()
    {
        _machine.Configure(Status.Postponed)
            .Permit(Action.Continue, Status.Reviewed);
    }

    private void ConfigureFixedState()
    {
        _machine.Configure(Status.Fixed)
            .PermitIf(_validateTrigger, Status.Verified, isResolved => isResolved)
            .PermitIf(_validateTrigger, Status.ReopenedState, isResolved => !isResolved)
            .Permit(Action.ReopenIssue, Status.ReopenedState);
    }

    private void ConfigureTerminalStates()
    {
        _machine.Configure(Status.Unreproducible)
            .Permit(Action.ConfirmClose, Status.Verified)
            .Permit(Action.ReopenIssue, Status.ReopenedState);

        _machine.Configure(Status.Declined)
            .Permit(Action.ReopenIssue, Status.ReopenedState);

        _machine.Configure(Status.DuplicateEntry)
            .Permit(Action.ReopenIssue, Status.ReopenedState);

        _machine.Configure(Status.Verified)
            .Permit(Action.ReopenIssue, Status.ReopenedState);
    }

    private void ConfigureReopenedState()
    {
        _machine.Configure(Status.ReopenedState)
            .Permit(Action.SendBackForReview, Status.Reviewed)
            .Permit(Action.StartWork, Status.Active);
    }

    private static string FormatTransition(StateMachine<Status, Action>.Transition t) =>
        $"{t.Source} --{t.Trigger}--> {t.Destination}";

    private void Execute(Action action) => _machine.Fire(action);
}

public static class Runner
{
    public static void Main()
    {
        var issue = new Issue();

        Console.WriteLine("Issue workflow demonstration");
        Console.WriteLine(issue.GenerateReport());
        issue.PerformReview();
        issue.StartWorking();
        issue.MarkAsFixed();
        issue.ValidateFix(false);
        issue.ReturnForReview();
        Console.WriteLine(issue.GenerateReport());
        foreach (var entry in issue.EventLog)
        {
            Console.WriteLine(entry);
        }
    }
}
