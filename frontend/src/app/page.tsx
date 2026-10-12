"use client";

import { useState } from "react";
import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  Bell,
  CalendarDays,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  CircleHelp,
  Clock3,
  Command,
  CreditCard,
  LayoutDashboard,
  Menu,
  MoreHorizontal,
  Plus,
  Scissors,
  Search,
  Settings2,
  Sparkles,
  Users,
  Wallet,
  X,
} from "lucide-react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

const revenue = [
  { day: "Seg", value: 860, last: 700 },
  { day: "Ter", value: 1120, last: 940 },
  { day: "Qua", value: 980, last: 1040 },
  { day: "Qui", value: 1460, last: 1100 },
  { day: "Sex", value: 1780, last: 1380 },
  { day: "Sáb", value: 2210, last: 1640 },
  { day: "Dom", value: 420, last: 610 },
];

const services = [
  { name: "Corte degradê", count: 68, color: "#b5f36d" },
  { name: "Barba completa", count: 42, color: "#79aaff" },
  { name: "Corte + barba", count: 31, color: "#b69cff" },
  { name: "Outros", count: 19, color: "#f0a76f" },
];

const appointments = [
  { time: "09:00", name: "Rafael Martins", service: "Corte degradê", barber: "Lucas", color: "bg-[#b5f36d]" },
  { time: "09:30", name: "André Costa", service: "Barba completa", barber: "Pedro", color: "bg-[#79aaff]" },
  { time: "10:00", name: "Bruno Almeida", service: "Corte + barba", barber: "Lucas", color: "bg-[#b69cff]" },
  { time: "10:30", name: "Caio Mendes", service: "Corte degradê", barber: "Marcos", color: "bg-[#f0a76f]" },
];

const metrics = [
  { label: "Faturamento do mês", value: "R$ 18.420", change: "+12,8%", positive: true, icon: Wallet, note: "vs. mês anterior" },
  { label: "Agendamentos", value: "284", change: "+8,4%", positive: true, icon: CalendarDays, note: "vs. mês anterior" },
  { label: "Clientes ativos", value: "1.248", change: "+4,2%", positive: true, icon: Users, note: "vs. mês anterior" },
  { label: "Ticket médio", value: "R$ 64,86", change: "-2,1%", positive: false, icon: CreditCard, note: "vs. mês anterior" },
];

type ChartMode = "area" | "bar";

export default function DashboardPage() {
  const [chartMode, setChartMode] = useState<ChartMode>("area");
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <div className="min-h-screen bg-ink text-[#f1f3f1]">
      <aside className="fixed inset-y-0 left-0 z-20 hidden w-[248px] border-r border-line bg-[#141715] lg:flex lg:flex-col">
        <div className="flex h-[76px] items-center gap-3 border-b border-line px-6">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-accent text-ink"><Scissors size={19} strokeWidth={2.5} /></div>
          <div><p className="text-[15px] font-bold tracking-tight">BARBER<span className="text-accent">CLOUD</span></p><p className="mt-0.5 text-[10px] tracking-[0.18em] text-muted">BUSINESS SUITE</p></div>
        </div>
        <div className="px-4 pt-6">
          <p className="px-3 pb-3 text-[10px] font-semibold uppercase tracking-[0.16em] text-[#606862]">Workspace</p>
          <div className="mb-5 flex items-center justify-between rounded-xl border border-line bg-panel px-3 py-3">
            <div className="flex items-center gap-3"><div className="flex h-9 w-9 items-center justify-center rounded-lg bg-[#353e36] text-accent"><Scissors size={17} /></div><div><p className="text-[12px] font-semibold">Avelino Barbearia</p><p className="mt-1 text-[10px] text-muted">Plano Professional</p></div></div>
            <ChevronDown size={15} className="text-muted" />
          </div>
        </div>
        <nav className="flex-1 px-4" aria-label="Navegação principal">
          <p className="px-3 pb-2 text-[10px] font-semibold uppercase tracking-[0.16em] text-[#606862]">Principal</p>
          <NavLink active icon={<LayoutDashboard size={17} />} label="Visão geral" />
          <NavLink icon={<CalendarDays size={17} />} label="Agendamentos" badge="8" />
          <NavLink icon={<Users size={17} />} label="Clientes" />
          <NavLink icon={<Scissors size={17} />} label="Serviços" />
          <NavLink icon={<Wallet size={17} />} label="Financeiro" />
          <div className="my-5 h-px bg-line" />
          <p className="px-3 pb-2 text-[10px] font-semibold uppercase tracking-[0.16em] text-[#606862]">Inteligência</p>
          <NavLink icon={<Sparkles size={17} />} label="CEO Copilot" badge="IA" />
          <NavLink icon={<Activity size={17} />} label="Relatórios" />
          <div className="my-5 h-px bg-line" />
          <NavLink icon={<Settings2 size={17} />} label="Configurações" />
        </nav>
        <div className="mx-4 mb-4 rounded-xl border border-line bg-panel p-3.5">
          <div className="mb-2 flex items-center gap-2"><CircleHelp size={15} className="text-accent" /><span className="text-xs font-medium">Precisa de ajuda?</span></div>
          <p className="text-[11px] leading-relaxed text-muted">Acesse nossa central de suporte e tutoriais.</p>
          <button className="mt-3 text-[11px] font-semibold text-accent">Central de ajuda <span aria-hidden="true">↗</span></button>
        </div>
        <div className="flex items-center gap-3 border-t border-line px-5 py-4">
          <div className="flex h-9 w-9 items-center justify-center rounded-full bg-[#414943] text-xs font-bold">MA</div>
          <div className="min-w-0 flex-1"><p className="truncate text-xs font-medium">Marcos Avelino</p><p className="mt-1 text-[10px] text-muted">Administrador</p></div>
          <MoreHorizontal size={18} className="text-muted" />
        </div>
      </aside>

      <div className="lg:pl-[248px]">
        <header className="sticky top-0 z-10 flex h-[76px] items-center justify-between border-b border-line bg-ink/95 px-5 backdrop-blur-md sm:px-8">
          <div className="flex items-center gap-3">
            <button className="rounded-lg p-2 text-muted hover:bg-panel lg:hidden" aria-label="Abrir menu" onClick={() => setMenuOpen(!menuOpen)}>{menuOpen ? <X size={19} /> : <Menu size={19} />}</button>
            <div className="hidden items-center gap-2 text-xs text-muted sm:flex"><span>Workspace</span><ChevronRight size={14} /><span className="text-[#eef0ee]">Visão geral</span></div>
            <p className="text-sm font-semibold sm:hidden">Visão geral</p>
          </div>
          <div className="flex items-center gap-2 sm:gap-3">
            <button className="hidden items-center gap-2 rounded-lg border border-line bg-panel px-3 py-2 text-[11px] text-muted md:flex" aria-label="Buscar"><Search size={14} /><span>Buscar qualquer coisa...</span><kbd className="ml-8 flex items-center gap-0.5 rounded border border-line px-1.5 py-0.5 text-[9px]"><Command size={9} /> K</kbd></button>
            <button className="rounded-lg border border-line p-2.5 text-muted hover:text-white md:hidden" aria-label="Buscar"><Search size={17} /></button>
            <button className="relative rounded-lg border border-line p-2.5 text-muted hover:text-white" aria-label="Notificações"><Bell size={17} /><span className="absolute right-2 top-2 h-1.5 w-1.5 rounded-full bg-accent" /></button>
            <div className="hidden h-8 w-px bg-line sm:block" />
            <button className="flex items-center gap-2 rounded-lg border border-line px-3 py-2 text-[11px] font-medium hover:bg-panel"><CalendarDays size={14} className="text-muted" /><span className="hidden sm:inline">01 — 31 Out, 2026</span><span className="sm:hidden">Out 2026</span><ChevronDown size={13} className="text-muted" /></button>
          </div>
        </header>

        {menuOpen && <div className="fixed inset-x-0 top-[76px] z-20 border-b border-line bg-[#141715] p-4 shadow-2xl lg:hidden"><NavLink active icon={<LayoutDashboard size={17} />} label="Visão geral" /><NavLink icon={<CalendarDays size={17} />} label="Agendamentos" /><NavLink icon={<Users size={17} />} label="Clientes" /><NavLink icon={<Scissors size={17} />} label="Serviços" /><NavLink icon={<Wallet size={17} />} label="Financeiro" /><NavLink icon={<Sparkles size={17} />} label="CEO Copilot" /></div>}

        <main className="mx-auto max-w-[1500px] px-5 py-8 sm:px-8 lg:px-10">
          <div className="mb-7 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div><div className="mb-2 flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-accent" /><p className="text-[10px] font-semibold uppercase tracking-[0.18em] text-accent">Quarta-feira, 14 de outubro</p></div><h1 className="text-[26px] font-semibold tracking-tight sm:text-[30px]">Bom dia, Marcos <span aria-hidden="true">✦</span></h1><p className="mt-1.5 text-[13px] text-muted">Aqui está o resumo da sua barbearia hoje.</p></div>
            <button className="flex w-fit items-center gap-2 rounded-lg bg-accent px-4 py-2.5 text-xs font-bold text-ink transition hover:bg-[#c8ff84]"><Plus size={15} strokeWidth={2.5} /> Novo agendamento</button>
          </div>

          <div className="mb-5 flex items-center gap-2 rounded-lg border border-[#b5f36d]/20 bg-[#b5f36d]/[0.06] px-3.5 py-2.5 text-[11px] text-[#c2d5b0]"><Sparkles size={14} className="shrink-0 text-accent" /><span><strong className="font-semibold text-accent">Visão demonstrativa</strong> · Os indicadores abaixo usam dados ilustrativos e ainda não estão conectados ao sistema da barbearia.</span></div>

          <section aria-label="Indicadores principais" className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {metrics.map(({ label, value, change, positive, icon: Icon, note }) => (
              <div key={label} className="rounded-xl border border-line bg-panel p-4 sm:p-5">
                <div className="flex items-center justify-between"><p className="text-[11px] font-medium text-muted">{label}</p><div className="rounded-lg bg-[#212622] p-2 text-[#a6b0a8]"><Icon size={15} /></div></div>
                <div className="mt-4 flex items-end justify-between gap-2"><p className="text-[25px] font-semibold tracking-tight">{value}</p><span className={`mb-1 flex items-center gap-0.5 text-[10px] font-semibold ${positive ? "text-accent" : "text-[#f18d83]"}`}>{positive ? <ArrowUpRight size={13} /> : <ArrowDownRight size={13} />}{change}</span></div>
                <p className="mt-1 text-[10px] text-[#667069]">{note}</p>
              </div>
            ))}
          </section>

          <section className="mt-5 grid grid-cols-1 gap-5 xl:grid-cols-[minmax(0,1.75fr)_minmax(310px,0.9fr)]">
            <div className="rounded-xl border border-line bg-panel p-5 sm:p-6">
              <div className="flex flex-wrap items-start justify-between gap-3"><div><p className="text-[13px] font-semibold">Visão financeira</p><p className="mt-1 text-[11px] text-muted">Acompanhe a receita da sua barbearia</p></div><div className="flex items-center gap-1 rounded-lg border border-line p-1"><button aria-pressed={chartMode === "area"} onClick={() => setChartMode("area")} className={`rounded-md px-2.5 py-1.5 text-[10px] ${chartMode === "area" ? "bg-[#303731] text-accent" : "text-muted"}`}>Área</button><button aria-pressed={chartMode === "bar"} onClick={() => setChartMode("bar")} className={`rounded-md px-2.5 py-1.5 text-[10px] ${chartMode === "bar" ? "bg-[#303731] text-accent" : "text-muted"}`}>Barras</button></div></div>
              <div className="mt-5 flex items-end gap-3"><p className="text-[28px] font-semibold tracking-tight">R$ 18.420</p><span className="mb-1 flex items-center gap-1 text-[10px] font-semibold text-accent"><ArrowUpRight size={13} /> 12,8%</span></div>
              <div className="mt-5 h-[220px] w-full">
                <ResponsiveContainer width="100%" height="100%">
                  {chartMode === "area" ? (
                    <AreaChart data={revenue} margin={{ top: 8, right: 2, left: -20, bottom: 0 }}>
                      <defs><linearGradient id="revFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#b5f36d" stopOpacity={0.2} /><stop offset="95%" stopColor="#b5f36d" stopOpacity={0} /></linearGradient></defs>
                      <CartesianGrid vertical={false} stroke="#292e2b" strokeDasharray="3 5" />
                      <XAxis dataKey="day" tickLine={false} axisLine={false} tick={{ fill: "#778078", fontSize: 10 }} dy={9} />
                      <YAxis tickLine={false} axisLine={false} tick={{ fill: "#778078", fontSize: 9 }} tickFormatter={(value: number) => `R$${value / 1000}k`} />
                      <Tooltip contentStyle={{ background: "#202421", border: "1px solid #343a35", borderRadius: 9, fontSize: 11 }} formatter={(value) => [`R$ ${Number(value).toLocaleString("pt-BR")}`, ""]} />
                      <Area type="monotone" dataKey="last" stroke="#59635b" strokeWidth={1.5} strokeDasharray="4 4" fill="transparent" />
                      <Area type="monotone" dataKey="value" stroke="#b5f36d" strokeWidth={2} fill="url(#revFill)" />
                    </AreaChart>
                  ) : (
                    <BarChart data={revenue} margin={{ top: 8, right: 2, left: -20, bottom: 0 }} barGap={5}>
                      <CartesianGrid vertical={false} stroke="#292e2b" strokeDasharray="3 5" />
                      <XAxis dataKey="day" tickLine={false} axisLine={false} tick={{ fill: "#778078", fontSize: 10 }} dy={9} />
                      <YAxis tickLine={false} axisLine={false} tick={{ fill: "#778078", fontSize: 9 }} tickFormatter={(value: number) => `R$${value / 1000}k`} />
                      <Tooltip contentStyle={{ background: "#202421", border: "1px solid #343a35", borderRadius: 9, fontSize: 11 }} formatter={(value) => [`R$ ${Number(value).toLocaleString("pt-BR")}`, ""]} />
                      <Bar dataKey="last" fill="#3b433d" radius={[3, 3, 0, 0]} />
                      <Bar dataKey="value" fill="#b5f36d" radius={[3, 3, 0, 0]} />
                    </BarChart>
                  )}
                </ResponsiveContainer>
              </div>
              <div className="mt-3 flex items-center justify-center gap-5 text-[10px] text-muted"><span className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-accent" />Este período</span><span className="flex items-center gap-2"><span className="h-1.5 w-1.5 rounded-full bg-[#59635b]" />Período anterior</span></div>
            </div>

            <div className="rounded-xl border border-line bg-panel p-5 sm:p-6">
              <div className="flex items-start justify-between"><div><p className="text-[13px] font-semibold">Serviços mais vendidos</p><p className="mt-1 text-[11px] text-muted">Distribuição do mês</p></div><button className="rounded-md p-1 text-muted hover:bg-[#252a26]" aria-label="Mais opções"><MoreHorizontal size={18} /></button></div>
              <div className="relative mt-2 h-[200px]">
                <ResponsiveContainer width="100%" height="100%"><PieChart><Pie data={services} dataKey="count" nameKey="name" innerRadius={60} outerRadius={83} paddingAngle={4} stroke="none">{services.map((entry) => <Cell key={entry.name} fill={entry.color} />)}</Pie><Tooltip contentStyle={{ background: "#202421", border: "1px solid #343a35", borderRadius: 9, fontSize: 11 }} /></PieChart></ResponsiveContainer>
                <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center"><span className="text-[25px] font-semibold">160</span><span className="mt-0.5 text-[10px] text-muted">serviços</span></div>
              </div>
              <div className="space-y-3">
                {services.map((service) => <div key={service.name} className="flex items-center justify-between text-[11px]"><span className="flex items-center gap-2 text-[#b2b9b4]"><span className="h-2 w-2 rounded-full" style={{ backgroundColor: service.color }} />{service.name}</span><span className="font-medium">{service.count}<span className="ml-2 text-muted">{Math.round(service.count / 160 * 100)}%</span></span></div>)}
              </div>
            </div>
          </section>

          <section className="mt-5 grid grid-cols-1 gap-5 xl:grid-cols-[minmax(0,1.65fr)_minmax(300px,1fr)]">
            <div className="rounded-xl border border-line bg-panel">
              <div className="flex items-center justify-between border-b border-line px-5 py-4 sm:px-6"><div><p className="text-[13px] font-semibold">Agenda de hoje</p><p className="mt-1 text-[11px] text-muted">Quarta-feira, 14 de outubro · 8 agendamentos</p></div><button className="flex items-center gap-1 text-[11px] font-semibold text-accent">Ver agenda <ChevronRight size={14} /></button></div>
              <div className="divide-y divide-line/70">
                {appointments.map((item) => <div key={item.time} className="flex items-center gap-3 px-5 py-3.5 sm:px-6"><span className="w-10 text-[11px] font-medium text-[#b6beb8]">{item.time}</span><div className={`h-8 w-1 rounded-full ${item.color}`} /><div className="min-w-0 flex-1"><p className="truncate text-[12px] font-medium">{item.name}</p><p className="mt-1 truncate text-[10px] text-muted">{item.service}</p></div><div className="hidden items-center gap-1.5 text-[10px] text-muted sm:flex"><div className="flex h-5 w-5 items-center justify-center rounded-full bg-[#303833] text-[8px] font-bold text-[#c1cbc3]">{item.barber[0]}</div>{item.barber}</div><span className="hidden rounded-md border border-[#b5f36d]/15 bg-[#b5f36d]/[0.06] px-2 py-1 text-[9px] font-medium text-accent md:block">Confirmado</span></div>)}
              </div>
              <div className="flex items-center justify-between border-t border-line px-5 py-3 text-[10px] text-muted sm:px-6"><span>Mostrando 4 de 8 agendamentos</span><div className="flex gap-1"><button className="rounded-md border border-line p-1.5 hover:bg-[#252a26]" aria-label="Anterior"><ChevronLeft size={13} /></button><button className="rounded-md border border-line p-1.5 hover:bg-[#252a26]" aria-label="Próximo"><ChevronRight size={13} /></button></div></div>
            </div>

            <div className="flex flex-col gap-5">
              <div className="overflow-hidden rounded-xl border border-[#b5f36d]/20 bg-[radial-gradient(ellipse_at_top_right,_rgba(181,243,109,0.09),_transparent_60%)] p-5 sm:p-6">
                <div className="mb-4 flex items-center gap-2"><div className="flex h-7 w-7 items-center justify-center rounded-lg bg-accent/15 text-accent"><Sparkles size={14} /></div><span className="text-[12px] font-semibold">CEO Copilot</span><span className="rounded-full border border-[#b5f36d]/20 px-1.5 py-0.5 text-[8px] font-bold uppercase tracking-wider text-accent">IA</span></div>
                <p className="text-[17px] font-semibold leading-snug">Sua quarta está no ritmo certo.</p>
                <p className="mt-2 text-[11px] leading-relaxed text-[#929b94]">O faturamento cresceu <span className="font-semibold text-accent">12,8%</span> neste mês. Clientes que retornam em até 30 dias gastam, em média, 2,4× mais.</p>
                <button className="mt-4 flex items-center gap-1.5 text-[11px] font-semibold text-accent">Conversar com o Copilot <ChevronRight size={13} /></button>
              </div>
              <div className="rounded-xl border border-line bg-panel p-5 sm:p-6">
                <div className="mb-4 flex items-center justify-between"><div><p className="text-[13px] font-semibold">Seu time</p><p className="mt-1 text-[11px] text-muted">Desempenho no mês</p></div><button className="text-[10px] font-semibold text-accent">Ver todos</button></div>
                {[{ name: "Lucas Ferreira", initials: "LF", clients: 86, value: "R$ 6.840", color: "#b5f36d", width: "82%" }, { name: "Pedro Santos", initials: "PS", clients: 72, value: "R$ 5.620", color: "#79aaff", width: "68%" }, { name: "Marcos Lima", initials: "ML", clients: 59, value: "R$ 4.310", color: "#b69cff", width: "52%" }].map((barber) => <div key={barber.name} className="mb-4 last:mb-0"><div className="mb-2 flex items-center justify-between"><div className="flex items-center gap-2.5"><div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#303833] text-[9px] font-bold" style={{ color: barber.color }}>{barber.initials}</div><div><p className="text-[10px] font-medium">{barber.name}</p><p className="mt-0.5 text-[9px] text-muted">{barber.clients} clientes atendidos</p></div></div><p className="text-[10px] font-semibold">{barber.value}</p></div><div className="h-1 w-full overflow-hidden rounded-full bg-[#292e2b]"><div className="h-full rounded-full" style={{ width: barber.width, backgroundColor: barber.color }} /></div></div>)}
              </div>
            </div>
          </section>

          <footer className="mt-8 flex flex-col justify-between gap-2 border-t border-line pt-4 text-[10px] text-[#657068] sm:flex-row"><span>© 2026 BARBERCLOUD · Gestão inteligente para barbearias</span><span className="flex items-center gap-1.5"><Clock3 size={11} /> Dados atualizados agora</span></footer>
        </main>
      </div>
    </div>
  );
}

function NavLink({ icon, label, active = false, badge }: { icon: React.ReactNode; label: string; active?: boolean; badge?: string }) {
  return <a href="#" className={`mb-1 flex items-center gap-3 rounded-lg px-3 py-2.5 text-[12px] transition ${active ? "bg-[#292f2a] font-semibold text-accent" : "text-[#9ba39d] hover:bg-[#202421] hover:text-white"}`}>{icon}<span className="flex-1">{label}</span>{badge && <span className={`rounded px-1.5 py-0.5 text-[9px] ${badge === "IA" ? "bg-accent/15 font-bold text-accent" : "bg-[#2e3430] text-[#aeb5af]"}`}>{badge}</span>}</a>;
}
