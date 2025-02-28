using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using QuixStreams.State.Storage;
using QuixStreams.Streaming.States;
using Xunit;

namespace QuixStreams.Streaming.UnitTests.States;

public class StreamStateManagerShould
{
    private StreamStateManager CreateStreamStateManager()
    {
        return new StreamStateManager("myStream", new InMemoryStorage(), NullLoggerFactory.Instance, "TEST - ");
    }
    
    [Fact]
    public void GetStates_ShouldReturnExcepted()
    {
        // Arrange
        var manager = CreateStreamStateManager();

        // Assert
        var states = manager.GetStates().ToArray();
        states.Length.Should().Be(0);
        
        // Act
        manager.GetDictionaryState("test");
        manager.GetDictionaryState("test2");
        manager.GetDictionaryState("test2"); // get it twice
        manager.GetDictionaryState("test3");
        manager.DeleteState("test3");
        
        // Assert
        states = manager.GetStates().ToArray();
        states.Should().BeEquivalentTo(new[] { "test", "test2" });
    }
    
    [Fact]
    public void GetState_ShouldReturnSameManagerAlways()
    {
        // Arrange
        var manager = CreateStreamStateManager();
        
        // Act
        var testStreamState = manager.GetDictionaryState("test");
        var testStreamState2 = manager.GetDictionaryState("test");
        
        // Assert
        testStreamState.Should().NotBeNull();
        testStreamState.Should().BeSameAs(testStreamState2);
    }
    
    [Fact]
    public void DeleteState_ShouldReturnExpected()
    {
        // Arrange
        var manager = CreateStreamStateManager();
        
        // Act & Assert
        manager.DeleteState("test").Should().BeFalse();
        var testStreamState = manager.GetDictionaryState("test");
        manager.DeleteState("test").Should().BeTrue();
        var testStreamState2 = manager.GetDictionaryState("test");
        
        testStreamState2.Should().NotBeNull();
        testStreamState2.Should().NotBeSameAs(testStreamState);
    }
    
    [Fact]
    public void DeleteStates_ShouldReturnExpected()
    {
        // Arrange
        var manager = CreateStreamStateManager();
        var testStreamState = manager.GetDictionaryState("test");
        var testStreamState2 = manager.GetDictionaryState("test2");
        
        // Act 
        var count = manager.DeleteStates();
        
        // Arrange
        count.Should().Be(2);
        manager.GetStates().Should().BeEmpty();
        manager.GetDictionaryState("test").Should().NotBeSameAs(testStreamState);
    }
}