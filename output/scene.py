# ==============================================================================
# EduMotion Studio - Automatically Generated Manim Scene Script
# Lesson: ද්විමය සංඛ්‍යා දශම බවට හැරවීම (10110110) (ID: 3a65e9dc772f4c55bb85b4d1af1a356c)
# Generated at: 2026-10-07 15:12:40 UTC
# ==============================================================================
from manim import *
import numpy as np

config.pixel_width = 1920
config.pixel_height = 1080
config.frame_rate = 30
config.background_color = "#0D1117"

class EduMotionLessonScene(Scene):
    def construct(self):
        # Global styling colors
        COLOR_PRIMARY = "#58A6FF"
        COLOR_ACCENT = "#7EE787"
        COLOR_TEXT = "#F0F6FC"
        COLOR_MUTED = "#8B949E"
        COLOR_BG_CARD = "#161B22"

        # Header Title
        title = Text("ද්විමය සංඛ්‍යා දශම බවට හැරවීම (10110110)", font_size=36, color=COLOR_TEXT)
        title.to_edge(UP, buff=0.6)
        self.play(FadeIn(title, shift=DOWN*0.3), run_time=0.8)
        self.wait(0.5)

        # ------------------------------------------------------------
        # Step 01: ද්විමය සංඛ්‍යාව ඉදිරිපත් කිරීම (Type: show_binary)
        # ------------------------------------------------------------
        # Visualizing binary bits: 10110110
        bits = []
        bit_boxes = []
        binary_str = "10110110"
        for i, char in enumerate(binary_str):
            box = RoundedRectangle(corner_radius=0.15, width=0.9, height=1.1, color=COLOR_PRIMARY, fill_color=COLOR_BG_CARD, fill_opacity=0.8)
            label = Text(char, font_size=40, font="monospace", weight=BOLD, color=COLOR_TEXT)
            group = VGroup(box, label)
            bits.append(group)
        self.binary_group = VGroup(*bits).arrange(RIGHT, buff=0.18)
        self.binary_group.move_to(UP * 1.2)
        self.play(LaggedStart(*[FadeIn(b, shift=DOWN*0.4) for b in self.binary_group], lag_ratio=0.08), run_time=1.0)
        self.wait(5.66)

        # ------------------------------------------------------------
        # Step 02: ස්ථානීය අගයන් දැක්වීම (Type: show_place_values)
        # ------------------------------------------------------------
        # Visualizing place values: [128, 64, 32, 16, 8, 4, 2, 1]
        place_values = [128, 64, 32, 16, 8, 4, 2, 1]
        pv_mobjects = []
        for i, val in enumerate(place_values):
            pv_text = Text(str(val), font_size=24, color=COLOR_MUTED)
            pv_text.next_to(self.binary_group[i], UP, buff=0.25)
            pv_mobjects.append(pv_text)
        self.place_values_group = VGroup(*pv_mobjects)
        self.play(LaggedStart(*[Write(pv) for pv in self.place_values_group], lag_ratio=0.06), run_time=0.9)
        self.wait(4.28)

        # ------------------------------------------------------------
        # Step 03: අගය එක වන ස්ථාන තෝරාගැනීම (Type: select_bits)
        # ------------------------------------------------------------
        # Highlighting active bits (bit = 1)
        highlight_anims = []
        for i, item in enumerate(self.binary_group):
            bit_label = item[1].text
            if bit_label == "1":
                highlight_anims.append(item[0].animate.set_stroke(color=COLOR_ACCENT, width=4))
                highlight_anims.append(self.place_values_group[i].animate.set_color(COLOR_ACCENT))
            else:
                highlight_anims.append(item.animate.set_opacity(0.35))
                highlight_anims.append(self.place_values_group[i].animate.set_opacity(0.35))
        self.play(*highlight_anims, run_time=0.9)
        self.wait(5.20)

        # ------------------------------------------------------------
        # Step 04: එකතු කිරීමේ ප්‍රකාශනය (Type: addition_expression)
        # ------------------------------------------------------------
        # Displaying sum equation: 128 + 32 + 16 + 4 + 2
        self.expr_text = MathTex("128 + 32 + 16 + 4 + 2", font_size=42, color=COLOR_TEXT)
        self.expr_text.move_to(DOWN * 0.8)
        self.play(Write(self.expr_text), run_time=1.1)
        self.wait(6.12)

        # ------------------------------------------------------------
        # Step 05: අවසාන පිළිතුර (Type: final_answer)
        # ------------------------------------------------------------
        # Transforming equation into final answer: 128 + 32 + 16 + 4 + 2 = 182
        full_eq = MathTex("128 + 32 + 16 + 4 + 2 = 182", font_size=46, color=COLOR_ACCENT)
        full_eq.move_to(DOWN * 0.8)
        box = SurroundingRectangle(full_eq, buff=0.25, color=COLOR_ACCENT, corner_radius=0.1)
        if hasattr(self, 'expr_text'):
            self.play(ReplacementTransform(self.expr_text, full_eq), Create(box), run_time=1.0)
        else:
            self.play(FadeIn(full_eq), Create(box), run_time=1.0)
        self.play(Circumscribe(box, color=COLOR_ACCENT, time_width=0.4), run_time=0.8)
        self.wait(2.50)

        # Lesson Conclusion
        self.wait(1.5)
        self.play(*[FadeOut(mob) for mob in self.mobjects], run_time=1.0)
        self.wait(0.5)
