import { ArrowDown, ArrowUp } from "lucide-react";
import clsx from "classnames";

interface Props {
  score: number;
  onVote: (value: 1 | -1) => void;
  disabled?: boolean;
}

export function VoteButtons({ score, onVote, disabled }: Props) {
  return (
    <div className="flex flex-col items-center gap-3 text-white/80">
      <button
        className={clsx(
          "rounded-full border border-white/10 bg-white/10 p-2 transition hover:border-brand-400/60 hover:bg-brand-400/20",
          disabled && "pointer-events-none opacity-30"
        )}
        onClick={() => onVote(1)}
        aria-label="Vote up"
      >
        <ArrowUp size={18} />
      </button>
      <span className="rounded-full bg-white/10 px-3 py-1 text-sm font-semibold tracking-wide text-accent-300">
        {score}
      </span>
      <button
        className={clsx(
          "rounded-full border border-white/10 bg-white/10 p-2 transition hover:border-brand-400/60 hover:bg-brand-400/20",
          disabled && "pointer-events-none opacity-30"
        )}
        onClick={() => onVote(-1)}
        aria-label="Vote down"
      >
        <ArrowDown size={18} />
      </button>
    </div>
  );
}
