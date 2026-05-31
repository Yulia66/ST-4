using IssueTracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IssueTrackerTests;

[TestClass]
public class IssueWorkflowTests
{
    [TestMethod]
    public void NewIssueStartsInCreatedState()
    {
        var issue = new Issue();

        Assert.AreEqual(Issue.Status.Created, issue.CurrentStatus);
    }

    [TestMethod]
    public void PerformReviewMovesToReviewed()
    {
        var issue = new Issue();

        issue.PerformReview();

        Assert.AreEqual(Issue.Status.Reviewed, issue.CurrentStatus);
    }

    [TestMethod]
    public void StartWorkingMovesReviewedIssueToActive()
    {
        var issue = CreateReviewedIssue();

        issue.StartWorking();

        Assert.AreEqual(Issue.Status.Active, issue.CurrentStatus);
    }

    [TestMethod]
    public void RequestDetailsFromReviewedMovesToAwaitingResponse()
    {
        var issue = CreateReviewedIssue();

        issue.RequestDetails();

        Assert.AreEqual(Issue.Status.AwaitingResponse, issue.CurrentStatus);
    }

    [TestMethod]
    public void SupplyDetailsReturnsIssueToReviewed()
    {
        var issue = CreateReviewedIssue();
        issue.RequestDetails();

        issue.SupplyDetails();

        Assert.AreEqual(Issue.Status.Reviewed, issue.CurrentStatus);
    }

    [TestMethod]
    public void SetPostponedFromReviewedMovesToPostponed()
    {
        var issue = CreateReviewedIssue();

        issue.SetPostponed();

        Assert.AreEqual(Issue.Status.Postponed, issue.CurrentStatus);
    }

    [TestMethod]
    public void ResumeWorkReturnsPostponedIssueToReviewed()
    {
        var issue = CreateReviewedIssue();
        issue.SetPostponed();

        issue.ResumeWork();

        Assert.AreEqual(Issue.Status.Reviewed, issue.CurrentStatus);
    }

    [TestMethod]
    public void MarkAsRejectedMovesToDeclined()
    {
        var issue = CreateReviewedIssue();

        issue.MarkAsRejected();

        Assert.AreEqual(Issue.Status.Declined, issue.CurrentStatus);
    }

    [TestMethod]
    public void MarkAsDuplicateMovesToDuplicateEntry()
    {
        var issue = CreateReviewedIssue();

        issue.MarkAsDuplicate();

        Assert.AreEqual(Issue.Status.DuplicateEntry, issue.CurrentStatus);
    }

    [TestMethod]
    public void MarkUnreproducibleMovesToUnreproducible()
    {
        var issue = CreateReviewedIssue();

        issue.MarkUnreproducible();

        Assert.AreEqual(Issue.Status.Unreproducible, issue.CurrentStatus);
    }

    [TestMethod]
    public void MarkAsFixedMovesToFixed()
    {
        var issue = CreateActiveIssue();

        issue.MarkAsFixed();

        Assert.AreEqual(Issue.Status.Fixed, issue.CurrentStatus);
    }

    [TestMethod]
    public void ValidateFixWithTrueVerifiesFixedIssue()
    {
        var issue = CreateFixedIssue();

        issue.ValidateFix(true);

        Assert.AreEqual(Issue.Status.Verified, issue.CurrentStatus);
    }

    [TestMethod]
    public void ValidateFixWithFalseReopensFixedIssue()
    {
        var issue = CreateFixedIssue();

        issue.ValidateFix(false);

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void ReopenIssueMovesVerifiedIssueToReopenedState()
    {
        var issue = CreateVerifiedIssue();

        issue.ReopenIssue();

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void ReturnForReviewMovesReopenedIssueToReviewed()
    {
        var issue = CreateVerifiedIssue();
        issue.ReopenIssue();

        issue.ReturnForReview();

        Assert.AreEqual(Issue.Status.Reviewed, issue.CurrentStatus);
    }

    [TestMethod]
    public void ReopenIssueMovesDuplicateEntryToReopenedState()
    {
        var issue = CreateReviewedIssue();
        issue.MarkAsDuplicate();

        issue.ReopenIssue();

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void ReopenIssueMovesDeclinedIssueToReopenedState()
    {
        var issue = CreateReviewedIssue();
        issue.MarkAsRejected();

        issue.ReopenIssue();

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void FinalizeCloseMovesUnreproducibleIssueToVerified()
    {
        var issue = CreateReviewedIssue();
        issue.MarkUnreproducible();

        issue.FinalizeClose();

        Assert.AreEqual(Issue.Status.Verified, issue.CurrentStatus);
    }

    [TestMethod]
    public void RequestDetailsFromActiveMovesToAwaitingResponse()
    {
        var issue = CreateActiveIssue();

        issue.RequestDetails();

        Assert.AreEqual(Issue.Status.AwaitingResponse, issue.CurrentStatus);
    }

    [TestMethod]
    public void SetPostponedFromActiveMovesToPostponed()
    {
        var issue = CreateActiveIssue();

        issue.SetPostponed();

        Assert.AreEqual(Issue.Status.Postponed, issue.CurrentStatus);
    }

    [TestMethod]
    public void StartWorkingFromAwaitingResponseMovesToActive()
    {
        var issue = CreateReviewedIssue();
        issue.RequestDetails();

        issue.StartWorking();

        Assert.AreEqual(Issue.Status.Active, issue.CurrentStatus);
    }

    [TestMethod]
    public void ReopenIssueMovesUnreproducibleToReopenedState()
    {
        var issue = CreateReviewedIssue();
        issue.MarkUnreproducible();

        issue.ReopenIssue();

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void StartWorkingCannotBeExecutedFromCreatedState()
    {
        var issue = new Issue();

        Assert.ThrowsException<InvalidOperationException>(() => issue.StartWorking());
    }

    [TestMethod]
    public void SupplyDetailsCannotBeExecutedFromReviewedState()
    {
        var issue = CreateReviewedIssue();

        Assert.ThrowsException<InvalidOperationException>(() => issue.SupplyDetails());
    }

    [TestMethod]
    public void ValidateFixCannotBeExecutedFromActiveState()
    {
        var issue = CreateActiveIssue();

        Assert.ThrowsException<InvalidOperationException>(() => issue.ValidateFix(true));
    }

    [TestMethod]
    public void FinalizeCloseCannotBeExecutedFromReviewedState()
    {
        var issue = CreateReviewedIssue();

        Assert.ThrowsException<InvalidOperationException>(() => issue.FinalizeClose());
    }

    [TestMethod]
    public void ExceptionMessageContainsActionNameForInvalidTransition()
    {
        var issue = new Issue();

        var exception = Assert.ThrowsException<InvalidOperationException>(() => issue.StartWorking());

        StringAssert.Contains(exception.Message, "StartWork");
    }

    [TestMethod]
    public void ExceptionMessageContainsStateNameForInvalidTransition()
    {
        var issue = CreateReviewedIssue();

        var exception = Assert.ThrowsException<InvalidOperationException>(() => issue.FinalizeClose());

        StringAssert.Contains(exception.Message, "Reviewed");
    }

    [TestMethod]
    public void CanExecuteReportsAvailableActions()
    {
        var issue = CreateReviewedIssue();

        Assert.IsTrue(issue.CanExecute(Issue.Action.StartWork));
        Assert.IsFalse(issue.CanExecute(Issue.Action.ConfirmClose));
    }

    [TestMethod]
    public void FixedIssueCanBeReopenedWithoutVerification()
    {
        var issue = CreateFixedIssue();

        issue.ReopenIssue();

        Assert.AreEqual(Issue.Status.ReopenedState, issue.CurrentStatus);
    }

    [TestMethod]
    public void NewIssueDoesNotAllowReopenAction()
    {
        var issue = new Issue();

        Assert.IsFalse(issue.CanExecute(Issue.Action.ReopenIssue));
    }

    [TestMethod]
    public void VerifiedStateIsTerminal()
    {
        var issue = CreateVerifiedIssue();

        Assert.IsTrue(issue.IsTerminal);
    }

    [TestMethod]
    public void DeclinedStateIsTerminal()
    {
        var issue = CreateReviewedIssue();
        issue.MarkAsRejected();

        Assert.IsTrue(issue.IsTerminal);
    }

    [TestMethod]
    public void DuplicateEntryStateIsTerminal()
    {
        var issue = CreateReviewedIssue();
        issue.MarkAsDuplicate();

        Assert.IsTrue(issue.IsTerminal);
    }

    [TestMethod]
    public void UnreproducibleStateIsNotTerminal()
    {
        var issue = CreateReviewedIssue();
        issue.MarkUnreproducible();

        Assert.IsFalse(issue.IsTerminal);
    }

    [TestMethod]
    public void ActiveStateIsNotTerminal()
    {
        var issue = CreateActiveIssue();

        Assert.IsFalse(issue.IsTerminal);
    }

    [TestMethod]
    public void EventLogStoresEachTransition()
    {
        var issue = new Issue();
        issue.PerformReview();
        issue.StartWorking();
        issue.MarkAsFixed();

        Assert.AreEqual(3, issue.EventLog.Count);
        StringAssert.Contains(issue.EventLog[0], "Created --Review--> Reviewed");
    }

    [TestMethod]
    public void EventLogTracksValidationTransition()
    {
        var issue = CreateFixedIssue();

        issue.ValidateFix(true);

        Assert.AreEqual(4, issue.EventLog.Count);
        StringAssert.Contains(issue.EventLog[^1], "Fixed --ValidateFix--> Verified");
    }

    [TestMethod]
    public void ToStringContainsCurrentStatus()
    {
        var issue = CreateFixedIssue();

        var description = issue.ToString();

        StringAssert.Contains(description, "Fixed");
    }

    [TestMethod]
    public void ToStringContainsLogEntriesLabel()
    {
        var issue = new Issue();

        var description = issue.ToString();

        StringAssert.Contains(description, "log entries");
    }

    [TestMethod]
    public void GenerateReportContainsTerminalFlagForVerifiedIssue()
    {
        var issue = CreateVerifiedIssue();

        var report = issue.GenerateReport();

        StringAssert.Contains(report, "terminal: True");
    }

    private static Issue CreateReviewedIssue()
    {
        var issue = new Issue();
        issue.PerformReview();
        return issue;
    }

    private static Issue CreateActiveIssue()
    {
        var issue = CreateReviewedIssue();
        issue.StartWorking();
        return issue;
    }

    private static Issue CreateFixedIssue()
    {
        var issue = CreateActiveIssue();
        issue.MarkAsFixed();
        return issue;
    }

    private static Issue CreateVerifiedIssue()
    {
        var issue = CreateFixedIssue();
        issue.ValidateFix(true);
        return issue;
    }
}
