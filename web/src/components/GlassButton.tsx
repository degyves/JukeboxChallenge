import type { ReactNode } from "react";
import clsx from "classnames";
import { motion, type HTMLMotionProps } from "framer-motion";

type Variant = "solid" | "outline" | "ghost" | "danger";

type Props = Omit<HTMLMotionProps<"button">, "children"> & {
  variant?: Variant;
  size?: "md" | "lg";
  children?: ReactNode;
};

const variantClasses: Record<Variant, string> = {
  solid:
    "bg-gradient-to-r from-brand-500 via-brand-400 to-accent-400 text-slate-950 shadow-glass hover:shadow-lg hover:shadow-accent-400/30",
  outline:
    "border border-white/10 bg-white/5 text-white/90 hover:border-brand-400/70 hover:bg-brand-400/10",
  ghost: "border border-transparent bg-transparent text-white/80 hover:border-white/10 hover:bg-white/5",
  danger:
    "bg-gradient-to-r from-rose-500 via-orange-500 to-amber-400 text-white shadow-lg shadow-rose-500/30 hover:shadow-rose-500/40"
};

export function GlassButton({ className, variant = "solid", size = "md", children, ...rest }: Props) {
  const base =
    "relative inline-flex items-center justify-center gap-2 rounded-2xl font-heading font-semibold tracking-wide transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-950 focus-visible:ring-brand-400 disabled:cursor-not-allowed disabled:opacity-60 will-change-transform";
  const sizeClass = size === "lg" ? "px-6 py-3 text-base" : "px-5 py-2.5 text-sm";
  const resolvedVariant: Variant = variant ?? "solid";
  return (
    <motion.button
      whileHover={{
        y: -2,
        scale: 1.03,
        boxShadow: "0 20px 45px -20px rgba(99, 102, 241, 0.45)"
      }}
      whileTap={{ scale: 0.98 }}
      transition={{ type: "spring", stiffness: 280, damping: 18, mass: 0.6 }}
      className={clsx(base, sizeClass, variantClasses[resolvedVariant], className)}
      {...rest}
    >
      <span className="relative z-10">{children as ReactNode}</span>
    </motion.button>
  );
}
