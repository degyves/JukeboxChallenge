import { QRCodeSVG } from "qrcode.react";
import { GlassButton } from "./GlassButton";

interface Props {
  roomCode: string;
  joinUrl: string;
  onCopy?: () => void;
}

export function QrCard({ roomCode, joinUrl, onCopy }: Props) {
  return (
    <div className="glass-card w-full space-y-4 p-6 text-center">
      <p className="section-title text-center">Invite guests</p>
      <p className="text-4xl font-semibold text-white">{roomCode}</p>
      <div className="mx-auto aspect-square w-full max-w-[12rem] rounded-2xl bg-white/10 p-3 shadow-glass">
        <QRCodeSVG
          value={joinUrl}
          fgColor="#38bdf8"
          bgColor="transparent"
          style={{ width: "100%", height: "100%" }}
        />
      </div>
      <GlassButton variant="outline" onClick={onCopy}>
        Copy link
      </GlassButton>
    </div>
  );
}
