import {
  ArrowRight,
  Calendar,
  CheckCircle2,
  Layers,
  Mail,
  Shield,
  TrendingUp,
  Users,
} from 'lucide-react'
import { CONTACT_EMAIL, site } from './content'

function Nav() {
  return (
    <header className="fixed top-0 z-50 w-full border-b border-surface-border/80 bg-surface/90 backdrop-blur-md">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <a href="#" className="flex items-center gap-2">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-sm font-bold text-white">
            FC
          </span>
          <span className="font-semibold text-white">{site.name}</span>
        </a>
        <a
          href="#contact"
          className="rounded-full bg-brand-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-500"
        >
          {site.hero.ctaPrimary}
        </a>
      </div>
    </header>
  )
}

function Hero() {
  return (
    <section className="relative overflow-hidden pt-32 pb-20 md:pt-40 md:pb-28">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_top,_rgba(16,185,129,0.12)_0%,_transparent_55%)]" />
      <div className="relative mx-auto max-w-6xl px-6">
        <p className="mb-4 inline-flex items-center gap-2 rounded-full border border-surface-border bg-surface-raised px-3 py-1 text-xs font-medium text-brand-400">
          <span className="h-1.5 w-1.5 rounded-full bg-brand-400" />
          {site.tagline}
        </p>
        <h1 className="max-w-4xl font-display text-5xl leading-[1.1] tracking-tight text-white md:text-6xl lg:text-7xl">
          {site.hero.headline}{' '}
          <em className="text-brand-400 not-italic">{site.hero.headlineAccent}</em>
        </h1>
        <p className="mt-6 max-w-2xl text-lg leading-relaxed text-slate-400 md:text-xl">
          {site.hero.subhead}
        </p>
        <div className="mt-10 flex flex-wrap gap-4">
          <a
            href="#contact"
            className="inline-flex items-center gap-2 rounded-full bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-500"
          >
            {site.hero.ctaPrimary}
            <ArrowRight className="h-4 w-4" />
          </a>
          <a
            href="#for-you"
            className="inline-flex items-center gap-2 rounded-full border border-surface-border px-6 py-3 font-medium text-slate-300 transition hover:border-slate-600 hover:text-white"
          >
            {site.hero.ctaSecondary}
          </a>
        </div>
      </div>
    </section>
  )
}

function ForYou() {
  return (
    <section id="for-you" className="border-t border-surface-border py-20 md:py-28">
      <div className="mx-auto max-w-6xl px-6">
        <div className="max-w-2xl">
          <h2 className="font-display text-4xl text-white md:text-5xl">{site.forYou.title}</h2>
          <p className="mt-4 text-lg text-slate-400">{site.forYou.subtitle}</p>
        </div>
        <div className="mt-12 grid gap-6 md:grid-cols-2">
          {site.forYou.segments.map((segment) => (
            <article
              key={segment.title}
              className="rounded-2xl border border-brand-600/30 bg-surface-raised p-6 md:p-8"
            >
              <p className="text-xs font-semibold uppercase tracking-wider text-brand-400">
                {segment.label}
              </p>
              <h3 className="mt-2 text-xl font-semibold text-white">{segment.title}</h3>
              <p className="mt-3 text-sm leading-relaxed text-slate-400">{segment.description}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

const icons = [TrendingUp, Shield, CheckCircle2, Calendar, Layers, Users]

function Problems() {
  return (
    <section id="problems" className="border-t border-surface-border py-20 md:py-28">
      <div className="mx-auto max-w-6xl px-6">
        <div className="max-w-2xl">
          <h2 className="font-display text-4xl text-white md:text-5xl">{site.problemsIntro.title}</h2>
          <p className="mt-4 text-lg text-slate-400">{site.problemsIntro.subtitle}</p>
        </div>
        <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {site.painPoints.map((point, i) => {
            const Icon = icons[i] ?? CheckCircle2
            return (
              <article
                key={point.title}
                className="rounded-2xl border border-surface-border bg-surface-raised p-6 transition hover:border-brand-600/40"
              >
                <div className="mb-4 flex h-10 w-10 items-center justify-center rounded-lg bg-brand-600/10 text-brand-400">
                  <Icon className="h-5 w-5" />
                </div>
                <h3 className="text-lg font-semibold text-white">{point.title}</h3>
                <p className="mt-2 text-sm leading-relaxed text-slate-400">{point.description}</p>
              </article>
            )
          })}
        </div>
      </div>
    </section>
  )
}

function CancellationSpotlight() {
  return (
    <section className="border-t border-surface-border py-20 md:py-28">
      <div className="mx-auto max-w-6xl px-6">
        <div className="grid items-center gap-12 lg:grid-cols-2">
          <div>
            <p className="text-sm font-medium uppercase tracking-wider text-brand-400">
              Critical when you grow
            </p>
            <h2 className="mt-2 font-display text-4xl text-white md:text-5xl">
              {site.cancellation.title}
            </h2>
            <p className="mt-4 text-slate-400">{site.cancellation.body}</p>
            <p className="mt-4 text-slate-300">{site.cancellation.solution}</p>
          </div>
          <div className="rounded-2xl border border-surface-border bg-surface-raised p-8">
            <div className="space-y-4">
              {[
                { label: 'Policy window', value: '2–4 hours before session' },
                { label: 'Late cancel detected', value: 'Automatic charge applied' },
                { label: 'Coach notified', value: 'Slot freed for rebooking' },
                { label: 'Client record', value: 'Session deducted per your rules' },
              ].map((row) => (
                <div
                  key={row.label}
                  className="flex items-center justify-between border-b border-surface-border pb-4 last:border-0 last:pb-0"
                >
                  <span className="text-sm text-slate-500">{row.label}</span>
                  <span className="text-sm font-medium text-white">{row.value}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

function Approach() {
  return (
    <section className="border-t border-surface-border py-20 md:py-28">
      <div className="mx-auto max-w-6xl px-6">
        <div className="rounded-3xl border border-brand-600/30 bg-gradient-to-br from-brand-600/10 to-surface-raised p-8 md:p-12">
          <h2 className="font-display text-4xl text-white md:text-5xl">{site.approach.title}</h2>
          <p className="mt-4 max-w-3xl text-lg text-slate-300">{site.approach.body}</p>
          <ul className="mt-8 space-y-3">
            {site.approach.points.map((point) => (
              <li key={point} className="flex items-start gap-3 text-slate-300">
                <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-brand-400" />
                {point}
              </li>
            ))}
          </ul>
        </div>
      </div>
    </section>
  )
}

function Contact() {
  return (
    <section id="contact" className="border-t border-surface-border py-20 md:py-28">
      <div className="mx-auto max-w-6xl px-6 text-center">
        <h2 className="font-display text-4xl text-white md:text-5xl">{site.cta.title}</h2>
        <p className="mx-auto mt-4 max-w-xl text-lg text-slate-400">{site.cta.body}</p>
        <a
          href={`mailto:${CONTACT_EMAIL}?subject=FitCore%20inquiry`}
          className="mt-8 inline-flex items-center gap-2 rounded-full bg-brand-600 px-8 py-4 text-lg font-medium text-white transition hover:bg-brand-500"
        >
          <Mail className="h-5 w-5" />
          {site.cta.button}
        </a>
      </div>
    </section>
  )
}

function Footer() {
  return (
    <footer className="border-t border-surface-border py-8">
      <div className="mx-auto max-w-6xl px-6 text-center text-sm text-slate-500">
        {site.footer}
      </div>
    </footer>
  )
}

export default function App() {
  return (
    <>
      <Nav />
      <main>
        <Hero />
        <ForYou />
        <Problems />
        <CancellationSpotlight />
        <Approach />
        <Contact />
      </main>
      <Footer />
    </>
  )
}
