import type { PropsWithChildren } from "react";
import clsx from "classnames";

type Props = PropsWithChildren<{
  className?: string;
  size?: "xl" | "lg" | "md";
  align?: "center" | "left" | "right";
}>;

export function GlitchTitle({ children, className, size = "xl", align = "center" }: Props) {
  const sizeClass =
    size === "xl"
      ? "text-4xl sm:text-5xl md:text-6xl"
      : size === "lg"
        ? "text-3xl sm:text-4xl"
        : "text-2xl sm:text-3xl";
  const alignClass = align === "center" ? "text-center" : align === "left" ? "text-left" : "text-right";

  return (
    <div className={clsx("relative", alignClass, className)}>
      <div className="inline-flex flex-col gap-2 rounded-[1.75rem] border border-white/10 bg-white/10 px-6 py-4 shadow-glass-soft backdrop-blur-md">
        <span className="text-xs font-semibold uppercase tracking-[0.4em] text-white/40">Live Session</span>
        <h1 className={clsx("font-heading font-semibold leading-tight text-white", sizeClass)}>{children}</h1>
      </div>
    </div>
  );
}
