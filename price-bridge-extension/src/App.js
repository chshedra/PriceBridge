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
      <button onClick={getPrice}>Get Price</button>
    </div>
  );
}

export default App;
