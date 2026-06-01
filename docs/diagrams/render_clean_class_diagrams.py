from __future__ import annotations

import html
import math
import re
from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path
from typing import Iterable
from xml.sax.saxutils import escape

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "docs" / "vp-tools" / "plugin-src" / "fml" / "vp" / "ClassDiagramPlugin.java"
OUT_DIR = ROOT / "docs" / "diagrams" / "final-export"
DRAWIO_PATH = ROOT / "docs" / "diagrams" / "class-diagrams-clean.drawio"

SCALE = 2
WIDTH = 2850
HEIGHT = 1150

COLORS = {
    "api": "#cfe8ff",
    "app": "#fff0bd",
    "domain": "#dff4df",
    "persistence": "#eadff8",
    "infra": "#f8dfd8",
}

LAYER_TITLES = {
    "api": "API",
    "app": "Application",
    "domain": "Domain",
    "persistence": "Persistence",
    "infra": "Infrastructure",
}


@dataclass
class Node:
    id: str
    layer: str
    name: str
    stereotype: str | None
    attrs: list[str]
    ops: list[str]
    x: int
    y: int
    w: int
    h: int

    @property
    def cx(self) -> float:
        return self.x + self.w / 2

    @property
    def cy(self) -> float:
        return self.y + self.h / 2


@dataclass
class Link:
    kind: str
    source: str
    target: str
    source_mult: str | None = None
    target_mult: str | None = None


@dataclass
class Diagram:
    title: str
    nodes: dict[str, Node]
    links: list[Link]


def java_strings(expr: str) -> list[str]:
    values: list[str] = []
    for match in re.finditer(r'"((?:\\.|[^"])*)"', expr):
        value = match.group(1)
        value = re.sub(r"\\u([0-9a-fA-F]{4})", lambda m: chr(int(m.group(1), 16)), value)
        value = value.replace(r"\"", '"').replace(r"\\", "\\")
        values.append(value)
    return values


def split_args(text: str) -> list[str]:
    args: list[str] = []
    start = 0
    depth = 0
    in_string = False
    escaped = False
    for i, ch in enumerate(text):
        if in_string:
            if escaped:
                escaped = False
            elif ch == "\\":
                escaped = True
            elif ch == '"':
                in_string = False
            continue
        if ch == '"':
            in_string = True
        elif ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
        elif ch == "," and depth == 0:
            args.append(text[start:i].strip())
            start = i + 1
    args.append(text[start:].strip())
    return args


def find_calls(body: str, name: str) -> Iterable[str]:
    token = name + "("
    pos = 0
    while True:
        start = body.find(token, pos)
        if start == -1:
            return
        i = start + len(token)
        depth = 1
        in_string = False
        escaped = False
        while i < len(body):
            ch = body[i]
            if in_string:
                if escaped:
                    escaped = False
                elif ch == "\\":
                    escaped = True
                elif ch == '"':
                    in_string = False
            else:
                if ch == '"':
                    in_string = True
                elif ch == "(":
                    depth += 1
                elif ch == ")":
                    depth -= 1
                    if depth == 0:
                        yield body[start + len(token):i]
                        pos = i + 1
                        break
            i += 1


def method_body(source: str, method: str) -> str:
    marker = f"private void {method}()"
    start = source.index(marker)
    brace = source.index("{", start)
    depth = 1
    i = brace + 1
    while i < len(source):
        if source[i] == "{":
            depth += 1
        elif source[i] == "}":
            depth -= 1
            if depth == 0:
                return source[brace + 1:i]
        i += 1
    raise ValueError(method)


def parse_diagram(title: str, method: str) -> Diagram:
    source = SOURCE.read_text(encoding="utf-8")
    body = method_body(source, method)
    nodes: dict[str, Node] = {}

    for call in find_calls(body, "clazz"):
        args = split_args(call)
        layer = args[0].strip()
        node_id = java_strings(args[1])[0]
        name = java_strings(args[2])[0]
        stereotype = None if args[3] == "null" else java_strings(args[3])[0]
        attrs = [] if args[4] == "null" else java_strings(args[4])
        ops = [] if args[5] == "null" else java_strings(args[5])
        x, y, w, h = [int(a.strip()) for a in args[-4:]]
        nodes[node_id] = Node(node_id, layer, name, stereotype, attrs, ops, x, y, w, h)

    links: list[Link] = []
    for kind in ["dependency", "realization"]:
        for call in find_calls(body, kind):
            parts = java_strings(call)
            links.append(Link(kind, parts[0], parts[1]))
    for kind in ["association", "composition"]:
        for call in find_calls(body, kind):
            parts = java_strings(call)
            links.append(Link(kind, parts[0], parts[1], parts[2], parts[3]))

    return Diagram(title, nodes, links)


def layer_key(layer: str) -> str:
    return {
        "api": "api",
        "app": "app",
        "domain": "domain",
        "persistence": "persistence",
        "infra": "infra",
    }.get(layer, "app")


@lru_cache(maxsize=None)
def load_font(name: str, size: int) -> ImageFont.FreeTypeFont:
    path = Path("C:/Windows/Fonts") / name
    return ImageFont.truetype(str(path), size * SCALE)


FONT = load_font("arial.ttf", 12)
FONT_SMALL = load_font("arial.ttf", 10)
FONT_BOLD = load_font("arialbd.ttf", 12)
FONT_TITLE = load_font("arialbd.ttf", 14)


def sc(value: float) -> int:
    return int(round(value * SCALE))


def text_fit(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont, max_width: int) -> str:
    if draw.textlength(text, font=font) <= max_width:
        return text
    result = text
    while len(result) > 4 and draw.textlength(result + "...", font=font) > max_width:
        result = result[:-1]
    return result + "..."


def shrink_font(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont, max_width: int) -> ImageFont.FreeTypeFont:
    if draw.textlength(text, font=font) <= max_width:
        return font
    is_bold = font in {FONT_BOLD, FONT_TITLE}
    base_size = max(8, font.size // SCALE)
    font_name = "arialbd.ttf" if is_bold else "arial.ttf"
    for size in range(base_size - 1, 7, -1):
        candidate = load_font(font_name, size)
        if draw.textlength(text, font=candidate) <= max_width:
            return candidate
    return load_font(font_name, 8)


def draw_text_center(draw: ImageDraw.ImageDraw, xy: tuple[int, int, int, int], text: str, font: ImageFont.FreeTypeFont, fill: str) -> None:
    x1, y1, x2, y2 = xy
    max_width = x2 - x1 - sc(8)
    font = shrink_font(draw, text, font, max_width)
    text = text_fit(draw, text, font, max_width)
    box = draw.textbbox((0, 0), text, font=font)
    tx = x1 + (x2 - x1 - (box[2] - box[0])) / 2
    ty = y1 + (y2 - y1 - (box[3] - box[1])) / 2 - sc(1)
    draw.text((tx, ty), text, font=font, fill=fill)


def dashed_line(draw: ImageDraw.ImageDraw, points: list[tuple[float, float]], fill: str, width: int) -> None:
    dash = sc(8)
    gap = sc(6)
    for a, b in zip(points, points[1:]):
        ax, ay = sc(a[0]), sc(a[1])
        bx, by = sc(b[0]), sc(b[1])
        length = math.hypot(bx - ax, by - ay)
        if length == 0:
            continue
        ux, uy = (bx - ax) / length, (by - ay) / length
        dist = 0.0
        while dist < length:
            end = min(dist + dash, length)
            draw.line((ax + ux * dist, ay + uy * dist, ax + ux * end, ay + uy * end), fill=fill, width=width)
            dist = end + gap


def solid_polyline(draw: ImageDraw.ImageDraw, points: list[tuple[float, float]], fill: str, width: int) -> None:
    scaled = [(sc(x), sc(y)) for x, y in points]
    draw.line(scaled, fill=fill, width=width, joint="curve")


def unit_direction(points: list[tuple[float, float]], at_end: bool) -> tuple[float, float]:
    a, b = (points[-2], points[-1]) if at_end else (points[1], points[0])
    dx, dy = b[0] - a[0], b[1] - a[1]
    length = math.hypot(dx, dy) or 1
    return dx / length, dy / length


def draw_open_arrow(draw: ImageDraw.ImageDraw, point: tuple[float, float], direction: tuple[float, float], fill: str) -> None:
    ux, uy = direction
    px, py = -uy, ux
    size = 11
    p = (sc(point[0]), sc(point[1]))
    left = (sc(point[0] - ux * size + px * size * 0.55), sc(point[1] - uy * size + py * size * 0.55))
    right = (sc(point[0] - ux * size - px * size * 0.55), sc(point[1] - uy * size - py * size * 0.55))
    draw.line([left, p, right], fill=fill, width=sc(1.4))


def draw_triangle(draw: ImageDraw.ImageDraw, point: tuple[float, float], direction: tuple[float, float], fill: str) -> None:
    ux, uy = direction
    px, py = -uy, ux
    size = 13
    pts = [
        (sc(point[0]), sc(point[1])),
        (sc(point[0] - ux * size + px * size * 0.7), sc(point[1] - uy * size + py * size * 0.7)),
        (sc(point[0] - ux * size - px * size * 0.7), sc(point[1] - uy * size - py * size * 0.7)),
    ]
    draw.polygon(pts, fill="white", outline=fill)


def draw_diamond(draw: ImageDraw.ImageDraw, point: tuple[float, float], direction: tuple[float, float], fill: str) -> None:
    ux, uy = direction
    px, py = -uy, ux
    size = 12
    pts = [
        (sc(point[0]), sc(point[1])),
        (sc(point[0] + ux * size * 0.75 + px * size * 0.55), sc(point[1] + uy * size * 0.75 + py * size * 0.55)),
        (sc(point[0] + ux * size * 1.5), sc(point[1] + uy * size * 1.5)),
        (sc(point[0] + ux * size * 0.75 - px * size * 0.55), sc(point[1] + uy * size * 0.75 - py * size * 0.55)),
    ]
    draw.polygon(pts, fill=fill, outline=fill)


def endpoint(node: Node, side: str, index: int, total: int) -> tuple[float, float]:
    if side in {"left", "right"}:
        y = node.y + node.h * (index + 1) / (total + 1)
        x = node.x if side == "left" else node.x + node.w
    else:
        x = node.x + node.w * (index + 1) / (total + 1)
        y = node.y if side == "top" else node.y + node.h
    return x, y


def sides(a: Node, b: Node) -> tuple[str, str]:
    dx = b.cx - a.cx
    dy = b.cy - a.cy
    if abs(dx) >= abs(dy):
        return ("right", "left") if dx >= 0 else ("left", "right")
    return ("bottom", "top") if dy >= 0 else ("top", "bottom")


def route(a: tuple[float, float], b: tuple[float, float], side_a: str, side_b: str) -> list[tuple[float, float]]:
    if side_a in {"left", "right"}:
        mid_x = (a[0] + b[0]) / 2
        return [a, (mid_x, a[1]), (mid_x, b[1]), b]
    mid_y = (a[1] + b[1]) / 2
    return [a, (a[0], mid_y), (b[0], mid_y), b]


def prepare_routes(diagram: Diagram) -> list[tuple[Link, list[tuple[float, float]], str, str]]:
    side_counts: dict[tuple[str, str], int] = {}
    choices: list[tuple[Link, str, str]] = []
    for link in diagram.links:
        if link.source not in diagram.nodes or link.target not in diagram.nodes:
            continue
        side_a, side_b = sides(diagram.nodes[link.source], diagram.nodes[link.target])
        choices.append((link, side_a, side_b))
        side_counts[(link.source, side_a)] = side_counts.get((link.source, side_a), 0) + 1
        side_counts[(link.target, side_b)] = side_counts.get((link.target, side_b), 0) + 1

    side_indexes: dict[tuple[str, str], int] = {}
    result: list[tuple[Link, list[tuple[float, float]], str, str]] = []
    for link, side_a, side_b in choices:
        a = diagram.nodes[link.source]
        b = diagram.nodes[link.target]
        idx_a = side_indexes.get((link.source, side_a), 0)
        idx_b = side_indexes.get((link.target, side_b), 0)
        side_indexes[(link.source, side_a)] = idx_a + 1
        side_indexes[(link.target, side_b)] = idx_b + 1
        start = endpoint(a, side_a, idx_a, side_counts[(link.source, side_a)])
        end = endpoint(b, side_b, idx_b, side_counts[(link.target, side_b)])
        result.append((link, route(start, end, side_a, side_b), side_a, side_b))
    return result


def draw_node(draw: ImageDraw.ImageDraw, node: Node) -> None:
    x, y, w, h = map(sc, (node.x, node.y, node.w, node.h))
    fill = COLORS[layer_key(node.layer)]
    draw.rounded_rectangle((x, y, x + w, y + h), radius=sc(2), fill=fill, outline="#1d1d1d", width=sc(1))

    cursor = y + sc(5)
    if node.stereotype:
        draw_text_center(draw, (x, cursor, x + w, cursor + sc(17)), f"<<{node.stereotype}>>", FONT_SMALL, "#222222")
        cursor += sc(17)
    draw_text_center(draw, (x, cursor, x + w, cursor + sc(21)), node.name, FONT_BOLD, "#111111")
    cursor += sc(24)

    if node.attrs:
        draw.line((x, cursor, x + w, cursor), fill="#333333", width=sc(1))
        cursor += sc(4)
        for attr in node.attrs:
            line = text_fit(draw, "+" + attr, FONT_SMALL, w - sc(10))
            draw.text((x + sc(6), cursor), line, font=FONT_SMALL, fill="#111111")
            cursor += sc(14)

    if node.ops:
        draw.line((x, cursor, x + w, cursor), fill="#333333", width=sc(1))
        cursor += sc(4)
        for op in node.ops:
            suffix = "" if op.endswith(")") else "()"
            line = text_fit(draw, "+" + op + suffix, FONT_SMALL, w - sc(10))
            draw.text((x + sc(6), cursor), line, font=FONT_SMALL, fill="#111111")
            cursor += sc(14)


def draw_layer_labels(draw: ImageDraw.ImageDraw) -> None:
    labels = [
        ("api", 40, 18, 300),
        ("app", 420, 18, 590),
        ("domain", 1090, 18, 560),
        ("persistence", 1730, 18, 370),
        ("infra", 2180, 18, 590),
    ]
    for key, x, y, w in labels:
        draw.rounded_rectangle((sc(x), sc(y), sc(x + w), sc(y + 38)), radius=sc(3), fill=COLORS[key], outline="#1d1d1d", width=sc(1))
        draw_text_center(draw, (sc(x), sc(y), sc(x + w), sc(y + 38)), LAYER_TITLES[key], FONT_TITLE, "#111111")


def render_png(diagram: Diagram, path: Path) -> None:
    image = Image.new("RGB", (WIDTH * SCALE, HEIGHT * SCALE), "white")
    draw = ImageDraw.Draw(image)
    draw_layer_labels(draw)

    routed = prepare_routes(diagram)
    for link, points, _, _ in routed:
        if link.kind in {"dependency", "realization"}:
            dashed_line(draw, points, "#222222", sc(1.2))
        else:
            solid_polyline(draw, points, "#222222", sc(1.2))

    for node in diagram.nodes.values():
        draw_node(draw, node)

    for link, points, _, _ in routed:
        end_dir = unit_direction(points, True)
        start_dir = unit_direction(points, False)
        if link.kind == "dependency":
            draw_open_arrow(draw, points[-1], end_dir, "#222222")
        elif link.kind == "realization":
            draw_triangle(draw, points[-1], end_dir, "#222222")
        elif link.kind == "composition":
            draw_diamond(draw, points[0], start_dir, "#111111")
        if link.source_mult:
            draw.text((sc(points[0][0] + 5), sc(points[0][1] + 5)), link.source_mult, font=FONT_SMALL, fill="#111111")
        if link.target_mult:
            draw.text((sc(points[-1][0] + 5), sc(points[-1][1] + 5)), link.target_mult, font=FONT_SMALL, fill="#111111")

    image.save(path, quality=95)


def svg_text(x: int, y: int, text: str, size: int = 11, weight: str = "normal", anchor: str = "start") -> str:
    return f'<text x="{x}" y="{y}" font-family="Arial" font-size="{size}" font-weight="{weight}" text-anchor="{anchor}" fill="#111">{html.escape(text)}</text>'


def svg_points(points: Iterable[tuple[float, float]]) -> str:
    return " ".join(f"{x:.1f},{y:.1f}" for x, y in points)


def svg_open_arrow(point: tuple[float, float], direction: tuple[float, float]) -> str:
    ux, uy = direction
    px, py = -uy, ux
    size = 11
    pts = [
        (point[0] - ux * size + px * size * 0.55, point[1] - uy * size + py * size * 0.55),
        point,
        (point[0] - ux * size - px * size * 0.55, point[1] - uy * size - py * size * 0.55),
    ]
    return f'<polyline points="{svg_points(pts)}" fill="none" stroke="#222" stroke-width="1.2"/>'


def svg_triangle(point: tuple[float, float], direction: tuple[float, float]) -> str:
    ux, uy = direction
    px, py = -uy, ux
    size = 13
    pts = [
        point,
        (point[0] - ux * size + px * size * 0.7, point[1] - uy * size + py * size * 0.7),
        (point[0] - ux * size - px * size * 0.7, point[1] - uy * size - py * size * 0.7),
    ]
    return f'<polygon points="{svg_points(pts)}" fill="white" stroke="#222" stroke-width="1.2"/>'


def svg_diamond(point: tuple[float, float], direction: tuple[float, float]) -> str:
    ux, uy = direction
    px, py = -uy, ux
    size = 12
    pts = [
        point,
        (point[0] + ux * size * 0.75 + px * size * 0.55, point[1] + uy * size * 0.75 + py * size * 0.55),
        (point[0] + ux * size * 1.5, point[1] + uy * size * 1.5),
        (point[0] + ux * size * 0.75 - px * size * 0.55, point[1] + uy * size * 0.75 - py * size * 0.55),
    ]
    return f'<polygon points="{svg_points(pts)}" fill="#111" stroke="#111" stroke-width="1.2"/>'


def svg_name_size(text: str, width: int) -> int:
    for size in range(12, 7, -1):
        if len(text) * size * 0.58 <= width - 8:
            return size
    return 8


def render_svg(diagram: Diagram, path: Path) -> None:
    parts: list[str] = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}">',
        '<rect width="100%" height="100%" fill="white"/>',
    ]
    for key, x, y, w in [("api", 40, 18, 300), ("app", 420, 18, 590), ("domain", 1090, 18, 560), ("persistence", 1730, 18, 370), ("infra", 2180, 18, 590)]:
        parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="38" rx="3" fill="{COLORS[key]}" stroke="#222"/>')
        parts.append(svg_text(x + w // 2, y + 25, LAYER_TITLES[key], 14, "bold", "middle"))

    routed = prepare_routes(diagram)
    for link, points, _, _ in routed:
        pts = svg_points(points)
        dash = ' stroke-dasharray="7 5"' if link.kind in {"dependency", "realization"} else ""
        parts.append(f'<polyline points="{pts}" fill="none" stroke="#222" stroke-width="1.2"{dash}/>')

    for node in diagram.nodes.values():
        fill = COLORS[layer_key(node.layer)]
        parts.append(f'<rect x="{node.x}" y="{node.y}" width="{node.w}" height="{node.h}" rx="2" fill="{fill}" stroke="#222"/>')
        cursor = node.y + 16
        if node.stereotype:
            parts.append(svg_text(node.x + node.w // 2, cursor, f"<<{node.stereotype}>>", 10, "normal", "middle"))
            cursor += 18
        parts.append(svg_text(node.x + node.w // 2, cursor, node.name, svg_name_size(node.name, node.w), "bold", "middle"))
        cursor += 18
        if node.attrs:
            parts.append(f'<line x1="{node.x}" y1="{cursor}" x2="{node.x + node.w}" y2="{cursor}" stroke="#333"/>')
            cursor += 13
            for attr in node.attrs:
                parts.append(svg_text(node.x + 6, cursor, "+" + attr, 10))
                cursor += 14
        if node.ops:
            parts.append(f'<line x1="{node.x}" y1="{cursor}" x2="{node.x + node.w}" y2="{cursor}" stroke="#333"/>')
            cursor += 13
            for op in node.ops:
                suffix = "" if op.endswith(")") else "()"
                parts.append(svg_text(node.x + 6, cursor, "+" + op + suffix, 10))
                cursor += 14

    for link, points, _, _ in routed:
        end_dir = unit_direction(points, True)
        start_dir = unit_direction(points, False)
        if link.kind == "dependency":
            parts.append(svg_open_arrow(points[-1], end_dir))
        elif link.kind == "realization":
            parts.append(svg_triangle(points[-1], end_dir))
        elif link.kind == "composition":
            parts.append(svg_diamond(points[0], start_dir))
        if link.source_mult:
            parts.append(svg_text(int(points[0][0] + 5), int(points[0][1] + 14), link.source_mult, 10))
        if link.target_mult:
            parts.append(svg_text(int(points[-1][0] + 5), int(points[-1][1] + 14), link.target_mult, 10))

    parts.append("</svg>")
    path.write_text("\n".join(parts), encoding="utf-8")


def drawio_label(node: Node) -> str:
    lines: list[str] = []
    if node.stereotype:
        lines.append(html.escape(f"<<{node.stereotype}>>"))
    lines.append(f"<b>{html.escape(node.name)}</b>")
    if node.attrs:
        lines.append("<hr>")
        lines.extend(html.escape("+" + attr) for attr in node.attrs)
    if node.ops:
        lines.append("<hr>")
        for op in node.ops:
            suffix = "" if op.endswith(")") else "()"
            lines.append(html.escape("+" + op + suffix))
    value = '<div style="text-align:center">' + "<br>".join(lines[:2]) + "</div>" + (
        '<div style="text-align:left">' + "<br>".join(lines[2:]) + "</div>" if len(lines) > 2 else ""
    )
    return escape(value, {'"': "&quot;"})


def drawio_edge_style(kind: str) -> str:
    base = "html=1;rounded=0;edgeStyle=orthogonalEdgeStyle;orthogonalLoop=1;jettySize=auto;orthogonal=1;"
    if kind == "dependency":
        return base + "dashed=1;endArrow=open;endFill=0;"
    if kind == "realization":
        return base + "dashed=1;endArrow=block;endFill=0;"
    if kind == "composition":
        return base + "endArrow=none;startArrow=diamond;startFill=1;"
    return base + "endArrow=none;"


def render_drawio(diagrams: list[Diagram], path: Path) -> None:
    diagram_xml: list[str] = []
    for page_index, diagram in enumerate(diagrams, 1):
        cells = ['<mxCell id="0"/>', '<mxCell id="1" parent="0"/>']
        for key, x, y, w in [("api", 40, 18, 300), ("app", 420, 18, 590), ("domain", 1090, 18, 560), ("persistence", 1730, 18, 370), ("infra", 2180, 18, 590)]:
            cell_id = f"{diagram.title}_{key}_label"
            style = f"rounded=0;whiteSpace=wrap;html=1;fillColor={COLORS[key]};strokeColor=#222222;fontStyle=1;fontSize=14;"
            cells.append(
                f'<mxCell id="{cell_id}" value="{LAYER_TITLES[key]}" style="{style}" vertex="1" parent="1">'
                f'<mxGeometry x="{x}" y="{y}" width="{w}" height="38" as="geometry"/></mxCell>'
            )
        for node in diagram.nodes.values():
            style = f"rounded=0;whiteSpace=wrap;html=1;fillColor={COLORS[layer_key(node.layer)]};strokeColor=#222222;spacing=6;fontSize=11;align=center;verticalAlign=top;"
            cells.append(
                f'<mxCell id="{diagram.title}_{node.id}" value="{drawio_label(node)}" style="{style}" vertex="1" parent="1">'
                f'<mxGeometry x="{node.x}" y="{node.y}" width="{node.w}" height="{node.h}" as="geometry"/></mxCell>'
            )
        for i, link in enumerate(diagram.links, 1):
            if link.source not in diagram.nodes or link.target not in diagram.nodes:
                continue
            value = ""
            if link.source_mult or link.target_mult:
                value = escape("   ".join(v for v in [link.source_mult, link.target_mult] if v))
            cells.append(
                f'<mxCell id="{diagram.title}_edge_{i}" value="{value}" style="{drawio_edge_style(link.kind)}" edge="1" parent="1" '
                f'source="{diagram.title}_{link.source}" target="{diagram.title}_{link.target}">'
                '<mxGeometry relative="1" as="geometry"/></mxCell>'
            )
        graph = (
            f'<mxGraphModel dx="2850" dy="1150" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" '
            f'arrows="1" fold="1" page="1" pageScale="1" pageWidth="{WIDTH}" pageHeight="{HEIGHT}" math="0" shadow="0">'
            "<root>" + "".join(cells) + "</root></mxGraphModel>"
        )
        diagram_xml.append(f'<diagram id="fml-{page_index}" name="{diagram.title}">{graph}</diagram>')
    mxfile = '<mxfile host="app.diagrams.net" agent="Codex" version="24.7.17">' + "".join(diagram_xml) + "</mxfile>"
    path.write_text(mxfile, encoding="utf-8")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    diagrams = [
        parse_diagram("Users", "createUsersDiagram"),
        parse_diagram("Projects", "createProjectsDiagram"),
        parse_diagram("Tasks", "createTasksDiagram"),
    ]
    for diagram in diagrams:
        render_png(diagram, OUT_DIR / f"{diagram.title}.png")
        render_svg(diagram, OUT_DIR / f"{diagram.title}.svg")
        print(OUT_DIR / f"{diagram.title}.png")
        print(OUT_DIR / f"{diagram.title}.svg")
    render_drawio(diagrams, DRAWIO_PATH)
    print(DRAWIO_PATH)


if __name__ == "__main__":
    main()
