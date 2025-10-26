import { Navigate, Route, Routes, BrowserRouter } from "react-router-dom";
import { LandingPage } from "./pages/LandingPage";
import { RoomPage } from "./pages/RoomPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/join/:code" element={<LandingPage />} />
        <Route path="/room/:code" element={<RoomPage />} />
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
