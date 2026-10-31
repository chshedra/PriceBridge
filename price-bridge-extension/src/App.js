import "./App.css";

function App() {
  const getPrice = async () => {
    const [tab] = await chrome.tabs.query({
      active: true,
      currentWindow: true,
    });

    await chrome.tabs.sendMessage(tab.id, {
      action: "GET_PRICE",
    });
  };

  return (
    <div className="App">
      <header className="App-header">
        <button onClick={getPrice}>Get Price</button>
      </header>
    </div>
  );
}

export default App;
