using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests;

[TestClass]
public class DevelopmentTaskTests
{
    #region Initial State Tests
    
    [TestMethod]
    public void TaskStartsInBacklogState()
    {
        var task = new DevelopmentTask();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Backlog, task.CurrentState);
        Assert.IsFalse(task.IsCompleted);
    }
    
    [TestMethod]
    public void NewTaskHasEmptyHistory()
    {
        var task = new DevelopmentTask();
        
        Assert.AreEqual(0, task.History.Count);
    }
    
    #endregion

    #region Happy Path Tests
    
    [TestMethod]
    public void StartAnalysis_MovesFromBacklogToAnalysis()
    {
        var task = new DevelopmentTask();
        
        task.StartAnalysis();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Analysis, task.CurrentState);
    }
    
    [TestMethod]
    public void ApproveAnalysis_MovesFromAnalysisToPlanned()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Analysis);
        
        task.ApproveAnalysis();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Planned, task.CurrentState);
    }
    
    [TestMethod]
    public void StartDevelopment_MovesFromPlannedToInDevelopment()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Planned);
        
        task.StartDevelopment();
        
        Assert.AreEqual(DevelopmentTask.TaskState.InDevelopment, task.CurrentState);
    }
    
    [TestMethod]
    public void CompleteDevelopment_MovesFromInDevelopmentToCodeReview()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.InDevelopment);
        
        task.CompleteDevelopment();
        
        Assert.AreEqual(DevelopmentTask.TaskState.CodeReview, task.CurrentState);
    }
    
    [TestMethod]
    public void RequestReview_MovesFromInDevelopmentToCodeReview()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.InDevelopment);
        
        task.CompleteDevelopment();
        task.RequestReview();
        
        Assert.AreEqual(DevelopmentTask.TaskState.CodeReview, task.CurrentState);
    }
    
    [TestMethod]
    public void ApproveReview_MovesFromCodeReviewToTesting()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        task.ApproveReview();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Testing, task.CurrentState);
    }
    
    [TestMethod]
    public void PassTests_MovesFromTestingToReadyForRelease()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Testing);
        
        task.PassTests();
        
        Assert.AreEqual(DevelopmentTask.TaskState.ReadyForRelease, task.CurrentState);
    }
    
    [TestMethod]
    public void Release_MovesFromReadyForReleaseToDone()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.ReadyForRelease);
        
        task.Release();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Done, task.CurrentState);
        Assert.IsTrue(task.IsCompleted);
    }
    
    #endregion

    #region Negative Path and Error Handling Tests
    
    [TestMethod]
    public void RequestChanges_MovesFromCodeReviewToFixing()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        task.RequestChanges();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Fixing, task.CurrentState);
    }
    
    [TestMethod]
    public void FixIssue_MovesFromFixingToCodeReview()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Fixing);
        
        task.FixIssue();
        
        Assert.AreEqual(DevelopmentTask.TaskState.CodeReview, task.CurrentState);
    }
    
    [TestMethod]
    public void FailTests_MovesFromTestingToFixing()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Testing);
        
        task.FailTests("Unit test failed");
        
        Assert.AreEqual(DevelopmentTask.TaskState.Fixing, task.CurrentState);
    }
    
    [TestMethod]
    public void Block_MovesFromPlannedToBlocked()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Planned);
        
        task.Block();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Blocked, task.CurrentState);
    }
    
    [TestMethod]
    public void Unblock_MovesFromBlockedToPlanned()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Blocked);
        
        task.Unblock();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Planned, task.CurrentState);
    }
    
    [TestMethod]
    public void Cancel_MovesFromBacklogToCancelled()
    {
        var task = new DevelopmentTask();
        
        task.Cancel();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Cancelled, task.CurrentState);
        Assert.IsTrue(task.IsCompleted);
    }
    
    [TestMethod]
    public void Reopen_MovesFromDoneToInDevelopment()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Done);
        
        task.Reopen();
        
        Assert.AreEqual(DevelopmentTask.TaskState.InDevelopment, task.CurrentState);
    }
    
    [TestMethod]
    public void Reopen_MovesFromReadyForReleaseToInDevelopment()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.ReadyForRelease);
        
        task.Reopen();
        
        Assert.AreEqual(DevelopmentTask.TaskState.InDevelopment, task.CurrentState);
    }
    
    #endregion

    #region Exception Tests (Invalid Transitions)
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ApproveAnalysis_FromBacklog_ThrowsException()
    {
        var task = new DevelopmentTask();
        
        task.ApproveAnalysis();
    }
    
    [TestMethod]
    public void StartDevelopment_FromBacklog_ThrowsInvalidOperation()
    {
        var task = new DevelopmentTask();
        
        Assert.ThrowsException<InvalidOperationException>(() => task.StartDevelopment());
    }
    
    [TestMethod]
    public void Release_FromTesting_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Testing);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.Release());
    }
    
    [TestMethod]
    public void PassTests_FromCodeReview_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.PassTests());
    }
    
    [TestMethod]
    public void Block_FromDone_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Done);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.Block());
    }
    
    [TestMethod]
    public void Cancel_FromDone_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Done);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.Cancel());
    }
    
    [TestMethod]
    public void ExceptionMessageContainsTriggerName()
    {
        var task = new DevelopmentTask();
        
        var exception = Assert.ThrowsException<InvalidOperationException>(() => task.Release());
        
        StringAssert.Contains(exception.Message, "Release");
    }
    
    #endregion

    #region CanFire Tests
    
    [TestMethod]
    public void CanFire_ReturnsTrueForValidTransitions()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Planned);
        
        Assert.IsTrue(task.CanTrigger(DevelopmentTask.TaskTrigger.StartDevelopment));
        Assert.IsTrue(task.CanTrigger(DevelopmentTask.TaskTrigger.Block));
    }
    
    [TestMethod]
    public void CanFire_ReturnsFalseForInvalidTransitions()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Planned);
        
        Assert.IsFalse(task.CanTrigger(DevelopmentTask.TaskTrigger.PassTests));
        Assert.IsFalse(task.CanTrigger(DevelopmentTask.TaskTrigger.Release));
    }
    
    [TestMethod]
    public void CanFire_OnCancelledState_NoTransitionsAllowed()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Cancelled);
        
        Assert.IsFalse(task.CanTrigger(DevelopmentTask.TaskTrigger.Reopen));
        Assert.IsFalse(task.CanTrigger(DevelopmentTask.TaskTrigger.StartAnalysis));
    }
    
    #endregion

    #region IsCompleted Tests
    
    [TestMethod]
    public void IsCompleted_ReturnsTrueForDoneState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Done);
        
        Assert.IsTrue(task.IsCompleted);
    }
    
    [TestMethod]
    public void IsCompleted_ReturnsTrueForCancelledState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Cancelled);
        
        Assert.IsTrue(task.IsCompleted);
    }
    
    [TestMethod]
    public void IsCompleted_ReturnsFalseForInDevelopment()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.InDevelopment);
        
        Assert.IsFalse(task.IsCompleted);
    }
    
    [TestMethod]
    public void IsCompleted_ReturnsFalseForBlocked()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Blocked);
        
        Assert.IsFalse(task.IsCompleted);
    }
    
    #endregion

    #region History Tracking Tests
    
    [TestMethod]
    public void HistoryRecordsEachTransition()
    {
        var task = new DevelopmentTask();
        
        task.StartAnalysis();
        task.ApproveAnalysis();
        task.StartDevelopment();
        
        Assert.AreEqual(3, task.History.Count);
    }
    
    [TestMethod]
    public void HistoryContainsTransitionDetails()
    {
        var task = new DevelopmentTask();
        
        task.StartAnalysis();
        
        StringAssert.Contains(task.History[0], "Backlog --StartAnalysis--> Analysis");
    }
    
    [TestMethod]
    public void FailTests_RecordsInHistory()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Testing);
        
        task.FailTests("Critical bug found");
        
        StringAssert.Contains(task.History[^1], "Testing --FailTests--> Fixing");
    }
    
    #endregion

    #region Report and ToString Tests
    
    [TestMethod]
    public void GetReport_ContainsCurrentState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        var report = task.GetReport();
        
        StringAssert.Contains(report, "CodeReview");
    }
    
    [TestMethod]
    public void GetReport_ContainsCompletedFlag()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Done);
        
        var report = task.GetReport();
        
        StringAssert.Contains(report, "Completed: True");
    }
    
    [TestMethod]
    public void ToString_ReturnsSameAsGetReport()
    {
        var task = new DevelopmentTask();
        
        Assert.AreEqual(task.GetReport(), task.ToString());
    }
    
    #endregion

    #region Additional Tests
    
    [TestMethod]
    public void StartAnalysis_FromAnalysis_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Analysis);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.StartAnalysis());
    }
    
    [TestMethod]
    public void CompleteDevelopment_FromPlanned_ThrowsException()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Planned);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.CompleteDevelopment());
    }
    
    [TestMethod]
    public void StartTesting_FromCodeReview_MovesToTesting()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        task.ApproveReview();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Testing, task.CurrentState);
    }
    
    [TestMethod]
    public void MultipleFailFixCycles_WorkCorrectly()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Testing);
        
        task.FailTests("Bug #1");
        Assert.AreEqual(DevelopmentTask.TaskState.Fixing, task.CurrentState);
        
        task.FixIssue();
        Assert.AreEqual(DevelopmentTask.TaskState.CodeReview, task.CurrentState);
        
        task.ApproveReview();
        Assert.AreEqual(DevelopmentTask.TaskState.Testing, task.CurrentState);
        
        task.PassTests();
        Assert.AreEqual(DevelopmentTask.TaskState.ReadyForRelease, task.CurrentState);
    }
    
    [TestMethod]
    public void BlockFromAnalysis_MovesToBlocked()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Analysis);
        
        task.Block();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Blocked, task.CurrentState);
    }
    
    [TestMethod]
    public void CancelFromAnalysis_MovesToCancelled()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Analysis);
        
        task.Cancel();
        
        Assert.AreEqual(DevelopmentTask.TaskState.Cancelled, task.CurrentState);
        Assert.IsTrue(task.IsCompleted);
    }
    
    [TestMethod]
    public void CannotPassTestsFromFixingState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.Fixing);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.PassTests());
    }
    
    [TestMethod]
    public void CannotReleaseFromInDevelopmentState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.InDevelopment);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.Release());
    }
    
    [TestMethod]
    public void CannotRequestReviewFromCodeReviewState()
    {
        var task = CreateTaskInState(DevelopmentTask.TaskState.CodeReview);
        
        Assert.ThrowsException<InvalidOperationException>(() => task.RequestReview());
    }
    
    #endregion

    #region Helper Methods
    
    private static DevelopmentTask CreateTaskInState(DevelopmentTask.TaskState targetState)
    {
        var task = new DevelopmentTask();
        
        switch (targetState)
        {
            case DevelopmentTask.TaskState.Backlog:
                return task;
                
            case DevelopmentTask.TaskState.Analysis:
                task.StartAnalysis();
                break;
                
            case DevelopmentTask.TaskState.Planned:
                task.StartAnalysis();
                task.ApproveAnalysis();
                break;
                
            case DevelopmentTask.TaskState.InDevelopment:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                break;
                
            case DevelopmentTask.TaskState.CodeReview:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                task.CompleteDevelopment();
                task.RequestReview();
                break;
                
            case DevelopmentTask.TaskState.Testing:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                task.CompleteDevelopment();
                task.RequestReview();
                task.ApproveReview();
                break;
                
            case DevelopmentTask.TaskState.Fixing:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                task.CompleteDevelopment();
                task.RequestReview();
                task.ApproveReview();
                task.StartTesting();
                task.FailTests("Test failed");
                break;
                
            case DevelopmentTask.TaskState.ReadyForRelease:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                task.CompleteDevelopment();
                task.RequestReview();
                task.ApproveReview();
                task.StartTesting();
                task.PassTests();
                break;
                
            case DevelopmentTask.TaskState.Done:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.StartDevelopment();
                task.CompleteDevelopment();
                task.RequestReview();
                task.ApproveReview();
                task.StartTesting();
                task.PassTests();
                task.Release();
                break;
                
            case DevelopmentTask.TaskState.Blocked:
                task.StartAnalysis();
                task.ApproveAnalysis();
                task.Block();
                break;
                
            case DevelopmentTask.TaskState.Cancelled:
                task.Cancel();
                break;
                
            default:
                throw new ArgumentException($"Unknown state: {targetState}");
        }
        
        return task;
    }
    
    #endregion
}